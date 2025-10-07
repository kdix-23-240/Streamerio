package handler

import (
	"crypto/rand"
	"encoding/json"
	"fmt"
	"io"
	"net/http"
	"sync"
	"time"

	"golang.org/x/net/websocket"

	"github.com/labstack/echo/v4"
	"github.com/oklog/ulid/v2"
	"streamerrio-backend/internal/service"
)

type WebSocketHandler struct {
	connections    map[string]*websocket.Conn
	mu             sync.RWMutex
	roomService    *service.RoomService
	sessionService *service.GameSessionService
	ulidEntropy    io.Reader
}

func NewWebSocketHandler() *WebSocketHandler {
	return &WebSocketHandler{
		connections: make(map[string]*websocket.Conn),
		ulidEntropy: ulid.Monotonic(rand.Reader, 0),
	}
}

// Unity接続管理
// /ws-unity に接続されたら、この関数が呼ばれる
func (h *WebSocketHandler) HandleUnityConnection(c echo.Context) error {
	s := websocket.Server{
		Handshake: func(cfg *websocket.Config, r *http.Request) error {
			// 全オリジン許可（必要ならここで厳密にチェック）
			return nil
		},
		Handler: func(ws *websocket.Conn) {
			defer ws.Close()

			// 接続登録（再接続の場合は同一 room_id を維持）
			requestedID := c.QueryParam("room_id")
			var id string
			if requestedID != "" {
				id = h.registerWithID(requestedID, ws, c)
			} else {
				id = h.registerNew(ws, c)
			}
			defer h.unregister(id, ws, c)

			// 接続直後に必ずログを出す
			c.Logger().Infof("Client connected: %s id=%s", c.Request().RemoteAddr, id)

			// 初期メッセージ（再接続時はタイプのみ変える）
			initType := "room_created"
			if requestedID != "" {
				initType = "room_ready"
			}
			payload := map[string]interface{}{
				"type":    initType,
				"room_id": id,
				"qr_code": "data:image/png;base64,...",
				"web_url": "https://example.com",
			}
			if err := h.SendEventToUnity(id, payload); err != nil {
				c.Logger().Errorf("initial send failed: %v", err)
				return
			}

			for {
				// Client からのメッセージを読み込む
				msg := ""
				err := websocket.Message.Receive(ws, &msg)

				// エラー処理
				// メッセージを受信できなかった場合は、接続を切断する
				if err != nil {
					if err == io.EOF {
						c.Logger().Infof("Client disconnected id=%s", id)
					} else {
						c.Logger().Errorf("receive failed: %v", err)
					}
					return
				}

				var incoming struct {
					Type string `json:"type"`
				}
				if err := json.Unmarshal([]byte(msg), &incoming); err != nil {
					continue
				}
				switch incoming.Type {
				case "game_end":
					if h.sessionService == nil {
						c.Logger().Warn("game_end received but sessionService not set")
						continue
					}
					if _, err := h.sessionService.EndGame(id); err != nil {
						c.Logger().Errorf("game end handling failed id=%s err=%v", id, err)
					}
				default:
					// その他のメッセージは現状無視
				}
			}
		},
	}

	s.ServeHTTP(c.Response(), c.Request())
	return nil
}

func (h *WebSocketHandler) RelayActionToUnity(c echo.Context) error {
	// リクエストボディをそのまま JSON として受け取り、Unity へ転送する
	var payload map[string]interface{}
	if err := c.Bind(&payload); err != nil {
		return c.JSON(http.StatusBadRequest, map[string]interface{}{"error": fmt.Sprintf("invalid json: %v", err)})
	}

	// room_idが含まれているか確認
	roomID, ok := payload["room_id"].(string)
	if !ok || roomID == "" {
		return c.JSON(http.StatusBadRequest, map[string]interface{}{"error": "room_id is required"})
	}

	// room_id を除去して転送用のペイロードを作成
	forward := make(map[string]interface{}, len(payload))
	for k, v := range payload {
		if k == "room_id" {
			continue
		}
		forward[k] = v
	}

	// Unity へ送信
	if err := h.SendEventToUnity(roomID, forward); err != nil {
		return c.JSON(http.StatusNotFound, map[string]interface{}{"error": err.Error()})
	}

	return c.JSON(http.StatusOK, map[string]interface{}{"status": "ok"})
}

// SetRoomService: 後から RoomService を注入
func (h *WebSocketHandler) SetRoomService(rs *service.RoomService) { h.roomService = rs }

// SetGameSessionService: ゲーム終了処理サービスを注入
func (h *WebSocketHandler) SetGameSessionService(gs *service.GameSessionService) {
	h.sessionService = gs
}

// registerNew: 新規接続用に新しい roomID を払い出して登録
func (h *WebSocketHandler) registerNew(ws *websocket.Conn, c echo.Context) string {
	id := ulid.MustNew(ulid.Timestamp(time.Now()), h.ulidEntropy).String()

	if h.roomService != nil {
		if err := h.roomService.CreateIfNotExists(id, "unity"); err != nil {
			c.Logger().Errorf("room db create failed id=%s err=%v", id, err)
		} else {
			c.Logger().Infof("room db created id=%s", id)
		}
	}

	h.mu.Lock()
	h.connections[id] = ws
	h.mu.Unlock()
	return id
}

// registerWithID: 指定 roomID で接続を登録（再接続時）
// 既存接続がある場合は置き換える
func (h *WebSocketHandler) registerWithID(id string, ws *websocket.Conn, c echo.Context) string {
	// 既存の DB レコードは触らない（既に存在している前提）。無い場合のみ作成。
	if h.roomService != nil {
		if err := h.roomService.CreateIfNotExists(id, "unity"); err != nil {
			c.Logger().Errorf("room db ensure failed id=%s err=%v", id, err)
		}
	}

	h.mu.Lock()
	h.connections[id] = ws
	h.mu.Unlock()
	c.Logger().Infof("room re-registered id=%s", id)
	return id
}

// unregister: 接続が同一の場合のみ削除（置換時の誤削除防止）
func (h *WebSocketHandler) unregister(id string, ws *websocket.Conn, c echo.Context) {
	h.mu.Lock()
	defer h.mu.Unlock()

	cur := h.connections[id]
	if cur == ws {
		delete(h.connections, id)
		c.Logger().Infof("Client unregistered id=%s", id)
	} else {
		// すでに別の接続に置き換わっている
		c.Logger().Infof("Skip unregister (replaced) id=%s", id)
	}
}

func (h *WebSocketHandler) SendEventToUnity(roomID string, payload interface{}) error {

	// 排他制御
	h.mu.RLock()
	defer h.mu.RUnlock()

	// 接続先にメッセージを送信
	client := h.connections[roomID]
	if client == nil {
		return fmt.Errorf("no websocket client for roomID=%s", roomID)
	}
	if err := websocket.JSON.Send(client, payload); err != nil {
		return fmt.Errorf("send failed: %v", err)
	}
	return nil
}

func (h *WebSocketHandler) ListClients(c echo.Context) error {
	h.mu.RLock()
	defer h.mu.RUnlock()
	ids := make([]string, 0, len(h.connections))
	for id := range h.connections {
		ids = append(ids, id)
	}
	return c.JSON(http.StatusOK, map[string]interface{}{"clients": ids})
}
