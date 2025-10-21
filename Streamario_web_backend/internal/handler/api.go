package handler

import (
    "log/slog"
    "net/http"
    "time"

    "streamerrio-backend/internal/service"

    "github.com/labstack/echo/v4"
)

// APIHandler: REST APIエンドポイント集約
type APIHandler struct {
    roomService    *service.RoomService
    eventService   *service.EventService
    viewerService  *service.ViewerService
    sessionService *service.GameSessionService
    logger         *slog.Logger
}

func NewAPIHandler(
    roomService *service.RoomService,
    eventService *service.EventService,
    viewerService *service.ViewerService,
    sessionService *service.GameSessionService,
    logger *slog.Logger,
) *APIHandler {
    if logger == nil {
        logger = slog.Default()
    }
    return &APIHandler{
        roomService:    roomService,
        eventService:   eventService,
        viewerService:  viewerService,
        sessionService: sessionService,
        logger:         logger,
    }
}

// GetRoom: ルーム情報取得
func (h *APIHandler) GetRoom(c echo.Context) error {
    roomID := c.Param("id")
    logger := h.logger.With(slog.String("handler", "GetRoom"), slog.String("room_id", roomID))

    logger.Debug("Fetching room")
    room, err := h.roomService.GetRoom(roomID)
    if err != nil {
        logger.Warn("Room not found", slog.Any("error", err))
        return c.JSON(http.StatusNotFound, map[string]string{"error": "room not found"})
    }

    logger.Info("Room retrieved successfully")
    return c.JSON(http.StatusOK, room)
}

// SendEvent: イベント送信
func (h *APIHandler) SendEvent(c echo.Context) error {
    roomID := c.Param("id")
    logger := h.logger.With(slog.String("handler", "SendEvent"), slog.String("room_id", roomID))

    var req struct {
        EventType string `json:"event_type"`
        ViewerID  string `json:"viewer_id"`
    }
    if err := c.Bind(&req); err != nil {
        logger.Warn("Invalid request body", slog.Any("error", err))
        return c.JSON(http.StatusBadRequest, map[string]string{"error": "invalid body"})
    }

    logger.Debug("Processing event",
        slog.String("event_type", req.EventType),
        slog.String("viewer_id", req.ViewerID))

    // ルーム存在確認
    if _, err := h.roomService.GetRoom(roomID); err != nil {
        logger.Warn("Room not found for event")
        return c.JSON(http.StatusNotFound, map[string]string{"error": "room not found"})
    }

    var viewerID *string
    if req.ViewerID != "" {
        viewerID = &req.ViewerID
    }

    res, err := h.eventService.ProcessEvent(roomID, req.EventType, viewerID)
    if err != nil {
        logger.Error("Event processing failed", slog.Any("error", err))
        return c.JSON(http.StatusInternalServerError, map[string]string{"error": err.Error()})
    }

    logger.Info("Event processed",
        slog.Int("current_count", res.CurrentCount),
        slog.Bool("triggered", res.EffectTriggered))

    return c.JSON(http.StatusOK, res)
}

// GetRoomStats: ルーム統計取得
func (h *APIHandler) GetRoomStats(c echo.Context) error {
    roomID := c.Param("id")
    logger := h.logger.With(slog.String("handler", "GetRoomStats"), slog.String("room_id", roomID))

    logger.Debug("Fetching room stats")

    if _, err := h.roomService.GetRoom(roomID); err != nil {
        logger.Warn("Room not found")
        return c.JSON(http.StatusNotFound, map[string]string{"error": "room not found"})
    }

    stats, err := h.eventService.GetRoomStats(roomID)
    if err != nil {
        logger.Error("Failed to get stats", slog.Any("error", err))
        return c.JSON(http.StatusInternalServerError, map[string]string{"error": err.Error()})
    }

    logger.Info("Stats retrieved", slog.Int("stat_count", len(stats)))

    return c.JSON(http.StatusOK, map[string]interface{}{
        "room_id": roomID,
        "stats":   stats,
        "time":    time.Now(),
    })
}

// GetRoomResults: ゲーム終了結果取得
func (h *APIHandler) GetRoomResults(c echo.Context) error {
    roomID := c.Param("id")
    logger := h.logger.With(slog.String("handler", "GetRoomResults"), slog.String("room_id", roomID))

    logger.Debug("Fetching room results")

    results, err := h.sessionService.GetRoomResult(roomID)
    if err != nil {
        logger.Error("Failed to get results", slog.Any("error", err))
        return c.JSON(http.StatusInternalServerError, map[string]string{"error": err.Error()})
    }

    logger.Info("Results retrieved")
    return c.JSON(http.StatusOK, results)
}

// GetViewerSummary: 視聴者別サマリー取得
func (h *APIHandler) GetViewerSummary(c echo.Context) error {
    roomID := c.Param("id")
    viewerID := c.Param("viewer_id")
    logger := h.logger.With(
        slog.String("handler", "GetViewerSummary"),
        slog.String("room_id", roomID),
        slog.String("viewer_id", viewerID))

    logger.Debug("Fetching viewer summary")

    summary, err := h.sessionService.GetViewerSummary(roomID, viewerID)
    if err != nil {
        logger.Error("Failed to get viewer summary", slog.Any("error", err))
        return c.JSON(http.StatusInternalServerError, map[string]string{"error": err.Error()})
    }

    logger.Info("Viewer summary retrieved", slog.Int("total", summary.Total))
    return c.JSON(http.StatusOK, summary)
}

// EndGame: ゲーム終了処理
func (h *APIHandler) EndGame(c echo.Context) error {
    roomID := c.Param("id")
    logger := h.logger.With(slog.String("handler", "EndGame"), slog.String("room_id", roomID))

    logger.Info("Ending game")

    summary, err := h.sessionService.EndGame(roomID)
    if err != nil {
        logger.Error("Failed to end game", slog.Any("error", err))
        return c.JSON(http.StatusInternalServerError, map[string]string{"error": err.Error()})
    }

    logger.Info("Game ended successfully")
    return c.JSON(http.StatusOK, summary)
}