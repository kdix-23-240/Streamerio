package handler

import (
	"net/http"
	"time"

	"streamerrio-backend/internal/model"
	"streamerrio-backend/internal/service"

	"github.com/labstack/echo/v4"
)

type APIHandler struct {
	roomService  *service.RoomService
	eventService *service.EventService
}

func NewAPIHandler(roomService *service.RoomService, eventService *service.EventService) *APIHandler {
	return &APIHandler{roomService: roomService, eventService: eventService}
}

// GET /api/rooms/:id
func (h *APIHandler) GetRoom(c echo.Context) error {
	id := c.Param("id")
	room, err := h.roomService.GetRoom(id)
	if err != nil {
		return c.JSON(http.StatusNotFound, map[string]string{"error": "room not found"})
	}
	return c.JSON(http.StatusOK, room)
}

// POST /api/rooms/:id/events
func (h *APIHandler) SendEvent(c echo.Context) error {
	roomID := c.Param("id")
	// Ensure room exists (auto-create if missing)
	if _, err := h.roomService.EnsureRoom(roomID); err != nil {
		return c.JSON(http.StatusInternalServerError, map[string]string{"error": "failed to ensure room"})
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

// GET /api/rooms/:id/stats
func (h *APIHandler) GetRoomStats(c echo.Context) error {
	roomID := c.Param("id")
	if _, err := h.roomService.EnsureRoom(roomID); err != nil {
		return c.JSON(http.StatusInternalServerError, map[string]string{"error": "failed to ensure room"})
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
