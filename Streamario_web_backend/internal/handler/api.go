package handler

import (
	"net/http"
	"time"

	"streamerrio-backend/internal/model"
	"streamerrio-backend/internal/service"

	"github.com/labstack/echo/v4"
)

// APIHandler: REST エンドポイント集約 (ルーム取得 / イベント送信 / 統計取得)
type APIHandler struct {
	roomService  *service.RoomService
	eventService *service.EventService
}

// NewAPIHandler: 依存するサービスを束ねて構築
func NewAPIHandler(roomService *service.RoomService, eventService *service.EventService) *APIHandler {
	return &APIHandler{roomService: roomService, eventService: eventService}
}

// GetRoom: ルーム情報取得 (存在しない場合 404)
func (h *APIHandler) GetRoom(c echo.Context) error {
	id := c.Param("id")
	room, err := h.roomService.GetRoom(id)
	if err != nil {
		return c.JSON(http.StatusNotFound, map[string]string{"error": "room not found"})
	}
	return c.JSON(http.StatusOK, room)
}

// SendEvent: イベントを受信し閾値チェックまで実施
func (h *APIHandler) SendEvent(c echo.Context) error {
	roomID := c.Param("id")
	// 自動生成せず 既存ルームのみ許容
	if _, err := h.roomService.GetRoom(roomID); err != nil {
		return c.JSON(http.StatusNotFound, map[string]string{"error": "room not found"})
	}
	var req struct {
		EventType string `json:"event_type"`
		ViewerID  string `json:"viewer_id"`
	}
	if err := c.Bind(&req); err != nil {
		return c.JSON(http.StatusBadRequest, map[string]string{"error": "invalid body"})
	}
	evType := model.EventType(req.EventType)
	var viewerID *string
	if req.ViewerID != "" {
		viewerID = &req.ViewerID
	}
	res, err := h.eventService.ProcessEvent(roomID, evType, viewerID)
	if err != nil {
		return c.JSON(http.StatusInternalServerError, map[string]string{"error": err.Error()})
	}
	return c.JSON(http.StatusOK, res)
}

// GetRoomStats: 現在のイベント種別ごとのカウントと閾値を返す
func (h *APIHandler) GetRoomStats(c echo.Context) error {
	roomID := c.Param("id")
	if _, err := h.roomService.GetRoom(roomID); err != nil {
		return c.JSON(http.StatusNotFound, map[string]string{"error": "room not found"})
	}
	stats, err := h.eventService.GetRoomStats(roomID)
	if err != nil {
		return c.JSON(http.StatusInternalServerError, map[string]string{"error": err.Error()})
	}
	return c.JSON(http.StatusOK, map[string]interface{}{
		"room_id": roomID,
		"stats":   stats,
		"time":    time.Now(),
	})
}
