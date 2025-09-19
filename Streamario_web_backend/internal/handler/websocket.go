package handler

import (
	"crypto/rand"
	"fmt"
	"io"
	"net/http"
	"sync"
	"time"
	"log"

	"golang.org/x/net/websocket"

	"github.com/labstack/echo/v4"
	"github.com/oklog/ulid/v2"
	"streamerrio-backend/internal/service"
)

// WebSocketHandler: Unity クライアントとの WebSocket 接続管理 (登録 / 送信 / 切断)
type WebSocketHandler struct {
	connections map[string]*websocket.Conn // roomID(=ULID) -> 接続
	mu          sync.RWMutex               // 接続マップ保護
	ulidEntropy io.Reader                  // ULID 生成用エントロピー
	roomService *service.RoomService       // ルームDB登録用 (nil の場合は登録スキップ)
}

// NewWebSocketHandler: ハンドラ生成
func NewWebSocketHandler() *WebSocketHandler {
    return &WebSocketHandler{
        connections: make(map[string]*websocket.Conn),
        ulidEntropy: ulid.Monotonic(rand.Reader, 0),
    }
}

// SetRoomService: 後から RoomService を注入
func (h *WebSocketHandler) SetRoomService(rs *service.RoomService) { h.roomService = rs }

// Unity接続管理
// /ws-unity に接続されたら、この関数が呼ばれる
// HandleUnityConnection: Unity クライアントの接続確立と受信ループ
func (h *WebSocketHandler) HandleUnityConnection(c echo.Context) error {
    s := websocket.Server{
        Handshake: func(cfg *websocket.Config, r *http.Request) error {
            return nil // オリジン許可 (必要に応じ制限)
        },
        Handler: func(ws *websocket.Conn) {
            defer ws.Close()

            id := Handler.register(ws)
            defer Handler.unregister(id)

            c.Logger().Infof("Client connected: %s id=%s", c.Request().RemoteAddr, id)

            // ここで DB 登録 (存在しなければ)
            if h.roomService != nil {
                if err := h.roomService.CreateIfNotExists(id, "unity"); err != nil {
                    log.Printf("room db create failed id=%s err=%v", id, err)
                } else {
                    log.Printf("room db created id=%s", id)
                }
            }

            payload := map[string]interface{}{
                "type":    "room_created",
                "room_id": id,
                "qr_code": "data:image/png;base64,...",
                "web_url": "https://example.com",
            }
            if err := Handler.SendEventToUnity(id, payload); err != nil {
                c.Logger().Errorf("initial send failed: %v", err)
                return
            }

            for {
                var msg string
                if err := websocket.Message.Receive(ws, &msg); err != nil {
                    if err == io.EOF {
                        c.Logger().Infof("Client disconnected id=%s", id)
                    } else {
                        c.Logger().Errorf("receive failed: %v", err)
                    }
                    return
                }
                // 現状メッセージ内容は未使用
            }
        },
    }

    s.ServeHTTP(c.Response(), c.Request())
    return nil
}

// register: 新規接続を管理マップに登録しULIDを返す
func (h *WebSocketHandler) register(ws *websocket.Conn) string {
	// ULIDで一意IDを生成（時系列順にソート可能）
	id := ulid.MustNew(ulid.Timestamp(time.Now()), h.ulidEntropy).String()

	// 排他制御
	h.mu.Lock()
	h.connections[id] = ws
	h.mu.Unlock()
	return id
}

// unregister: 接続マップから削除
func (h *WebSocketHandler) unregister(id string) {
	// 排他制御
	h.mu.Lock()
	defer h.mu.Unlock()

	// 接続IDを削除
	delete(h.connections, id)
}

// SendEventToUnity exports unified sending for adapter usage.
// SendEventToUnity: 指定 roomID の接続へ JSON 送信
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

// ListClients: 現在接続中の roomID 一覧を返す
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
