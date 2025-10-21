package handler

import (
    "crypto/rand"
    "fmt"
    "io"
    "log/slog"
    "net/http"
    "sync"
    "time"

    "streamerrio-backend/internal/service"

    "github.com/labstack/echo/v4"
    "github.com/oklog/ulid/v2"
    "golang.org/x/net/websocket"
)

// WebSocketHandler: Unity WebSocket接続管理
type WebSocketHandler struct {
    connections map[string]*websocket.Conn
    mu          sync.RWMutex
    ulidEntropy io.Reader
    roomService *service.RoomService
    logger      *slog.Logger
}

func NewWebSocketHandler(roomService *service.RoomService, logger *slog.Logger) *WebSocketHandler {
    if logger == nil {
        logger = slog.Default()
    }
    return &WebSocketHandler{
        connections: make(map[string]*websocket.Conn),
        ulidEntropy: ulid.Monotonic(rand.Reader, 0),
        roomService: roomService,
        logger:      logger,
    }
}

// HandleUnityConnection: Unity接続確立
func (h *WebSocketHandler) HandleUnityConnection(c echo.Context) error {
    logger := h.logger.With(slog.String("handler", "WebSocket"))
    
    s := websocket.Server{
        Handshake: func(cfg *websocket.Config, r *http.Request) error {
            logger.Debug("WebSocket handshake", slog.String("origin", r.Header.Get("Origin")))
            return nil
        },
        Handler: func(ws *websocket.Conn) {
            defer ws.Close()

            roomID := h.register(ws)
            defer h.unregister(roomID)

            connLogger := logger.With(
                slog.String("room_id", roomID),
                slog.String("remote_addr", c.Request().RemoteAddr))

            connLogger.Info("Unity client connected")

            // DB登録
            if err := h.roomService.CreateIfNotExists(roomID, "unity"); err != nil {
                connLogger.Error("Failed to create room in DB", slog.Any("error", err))
            } else {
                connLogger.Info("Room registered in DB")
            }

            // 初期メッセージ送信
            payload := map[string]interface{}{
                "type":    "room_created",
                "room_id": roomID,
                "qr_code": fmt.Sprintf("https://example.com/room/%s/qr", roomID),
                "web_url": fmt.Sprintf("https://example.com/room/%s", roomID),
            }
            if err := h.SendEventToUnity(roomID, payload); err != nil {
                connLogger.Error("Failed to send initial message", slog.Any("error", err))
                return
            }
            connLogger.Debug("Initial message sent")

            // メッセージ受信ループ
            for {
                var msg map[string]interface{}
                if err := websocket.JSON.Receive(ws, &msg); err != nil {
                    if err == io.EOF {
                        connLogger.Info("Unity client disconnected (EOF)")
                    } else {
                        connLogger.Error("Receive error", slog.Any("error", err))
                    }
                    return
                }
                connLogger.Debug("Message received from Unity", slog.Any("msg", msg))
                
                // メッセージハンドリング（将来の拡張用）
                if msgType, ok := msg["type"].(string); ok {
                    connLogger.Debug("Processing message", slog.String("type", msgType))
                }
            }
        },
    }

    s.ServeHTTP(c.Response(), c.Request())
    return nil
}

// register: 新規接続登録
func (h *WebSocketHandler) register(ws *websocket.Conn) string {
    id := ulid.MustNew(ulid.Timestamp(time.Now()), h.ulidEntropy).String()

    h.mu.Lock()
    h.connections[id] = ws
    h.mu.Unlock()

    h.logger.Debug("Connection registered",
        slog.String("room_id", id),
        slog.Int("total_connections", len(h.connections)))

    return id
}

// unregister: 接続削除
func (h *WebSocketHandler) unregister(id string) {
    h.mu.Lock()
    delete(h.connections, id)
    remaining := len(h.connections)
    h.mu.Unlock()

    h.logger.Debug("Connection unregistered",
        slog.String("room_id", id),
        slog.Int("remaining_connections", remaining))
}

// SendEventToUnity: 指定ルームへメッセージ送信
func (h *WebSocketHandler) SendEventToUnity(roomID string, payload map[string]interface{}) error {
    h.mu.RLock()
    client := h.connections[roomID]
    h.mu.RUnlock()

    if client == nil {
        h.logger.Warn("No WebSocket client found", slog.String("room_id", roomID))
        return fmt.Errorf("no websocket client for roomID=%s", roomID)
    }

    logger := h.logger.With(
        slog.String("room_id", roomID),
        slog.Any("payload_type", payload["type"]))

    logger.Debug("Sending message to Unity")

    if err := websocket.JSON.Send(client, payload); err != nil {
        logger.Error("Failed to send message", slog.Any("error", err))
        return fmt.Errorf("send failed: %w", err)
    }

    logger.Debug("Message sent successfully")
    return nil
}

// ListClients: 接続中クライアント一覧
func (h *WebSocketHandler) ListClients(c echo.Context) error {
    h.mu.RLock()
    ids := make([]string, 0, len(h.connections))
    for id := range h.connections {
        ids = append(ids, id)
    }
    h.mu.RUnlock()

    h.logger.Debug("Listing clients", slog.Int("count", len(ids)))

    return c.JSON(http.StatusOK, map[string]interface{}{
        "clients": ids,
        "count":   len(ids),
    })
}