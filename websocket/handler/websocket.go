package handler

import (
	"crypto/rand"
	"fmt"
	"io"
	"net/http"
	"sync"
	"time"

	"websocket/service"

	"golang.org/x/net/websocket"

	"github.com/labstack/echo/v4"
	"github.com/oklog/ulid/v2"
)

type WebSocketHandler struct {
	connections map[string]*websocket.Conn
	mu          sync.RWMutex
	roomService *service.RoomService
	ulidEntropy io.Reader
}

func NewWebSocketHandler() *WebSocketHandler {
	return &WebSocketHandler{
		connections: make(map[string]*websocket.Conn),
		roomService: service.NewRoomService(),
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

			// 接続登録
			id := h.register(ws)
			defer h.unregister(id)

			// 接続直後に必ずログを出す
			c.Logger().Infof("Client connected: %s id=%s", c.Request().RemoteAddr, id)

			// IDを通知（JSON を送信）
			if err := h.sendEventToUnity(id, map[string]interface{}{
				// TODO: 本番環境のURLに変更する
				"type":    "room_created",
				"room_id": id,
				"qr_code": "data:image/png;base64,...",
				"web_url": "https://example.com",
			}); err != nil {
				c.Logger().Errorf("initial send failed: %v", err)
				return
			} else {
				c.Logger().Info("initial send success")
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
	if err := h.sendEventToUnity(roomID, forward); err != nil {
		return c.JSON(http.StatusNotFound, map[string]interface{}{"error": err.Error()})
	}

	return c.JSON(http.StatusOK, map[string]interface{}{"status": "ok"})
}

func (h *WebSocketHandler) register(ws *websocket.Conn) string {
	// ULIDで一意IDを生成（時系列順にソート可能）
	id := ulid.MustNew(ulid.Timestamp(time.Now()), h.ulidEntropy).String()

	// 排他制御
	h.mu.Lock()
	h.connections[id] = ws
	h.mu.Unlock()
	return id
}

func (h *WebSocketHandler) unregister(id string) {
	// 排他制御
	h.mu.Lock()
	defer h.mu.Unlock()

	// 接続IDを削除
	delete(h.connections, id)
}

func (h *WebSocketHandler) sendEventToUnity(roomID string, payload interface{}) error {
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

var Handler *WebSocketHandler = NewWebSocketHandler()
