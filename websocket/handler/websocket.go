package handler

import (
	"fmt"
	"io"
	"net/http"
	"strconv"
	"sync"

	"websocket/service"

	"golang.org/x/net/websocket"

	"github.com/labstack/echo/v4"
)

type WebSocketHandler struct {
	connections map[string]*websocket.Conn
	mu          sync.RWMutex
	roomService *service.RoomService
}

func NewWebSocketHandler() *WebSocketHandler {
	return &WebSocketHandler{
		connections: make(map[string]*websocket.Conn),
		roomService: service.NewRoomService(),
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
			id := Handler.Register(ws)
			defer Handler.Unregister(id)

			// 接続直後に必ずログを出す
			c.Logger().Infof("Client connected: %s id=%s", c.Request().RemoteAddr, id)

			// IDを通知（JSON を送信）
			if err := Handler.SendEventToUnity(id, map[string]interface{}{
				"type":    "room_created",
				"room_id": id,
				"qr_code": "data:image/png;base64,...",
				"web_url": "https://example.com",
			}); err != nil {
				c.Logger().Errorf("initial send failed: %v", err)
				return
			} else {
				c.Logger().Info("Greeting sent")
			}

			for {
				// Client からのメッセージを読み込む
				msg := ""
				err := websocket.Message.Receive(ws, &msg)
				if err != nil {
					if err == io.EOF {
						c.Logger().Infof("Client disconnected id=%s", id)
					} else {
						c.Logger().Errorf("receive failed: %v", err)
					}
					return
				}

				// 受け取ったメッセージをログ出力
				c.Logger().Infof("Received from client: %s", msg)

				// Client からのメッセージを元に返すメッセージを作成し送信する
				err = sendMessageToClient(ws, fmt.Sprintf("Server: id=%s \"%s\" received!", id, msg))
				if err != nil {
					c.Logger().Errorf("send failed: %v", err)
					return
				}
			}
		},
	}

	s.ServeHTTP(c.Response(), c.Request())
	return nil
}

func (h *WebSocketHandler) Register(ws *websocket.Conn) string {
	// 排他制御
	h.mu.Lock()
	defer h.mu.Unlock()

	// 接続IDを生成
	id := len(h.connections) + 1
	h.connections[strconv.Itoa(id)] = ws
	return strconv.Itoa(id)
}

func (h *WebSocketHandler) Unregister(id string) {
	// 排他制御
	h.mu.Lock()
	defer h.mu.Unlock()

	// 接続IDを削除
	delete(h.connections, id)
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

var Handler = NewWebSocketHandler()
