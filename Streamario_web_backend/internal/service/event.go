package service

import (
    "fmt"
    "log/slog"

    "streamerrio-backend/internal/config"
    "streamerrio-backend/internal/model"
    "streamerrio-backend/internal/repository"
    "streamerrio-backend/pkg/counter"
)

type WebSocketSender interface {
    SendEventToUnity(roomID string, payload map[string]interface{}) error
}

type EventService struct {
    counter    counter.Counter
    eventRepo  repository.EventRepository
    wsHandler  WebSocketSender
    gameConfig *config.GameConfig
    logger     *slog.Logger
}

func NewEventService(
    counter counter.Counter,
    eventRepo repository.EventRepository,
    wsHandler WebSocketSender,
    gameConfig *config.GameConfig,
    logger *slog.Logger,
) *EventService {
    if logger == nil {
        logger = slog.Default()
    }
    return &EventService{
        counter:    counter,
        eventRepo:  eventRepo,
        wsHandler:  wsHandler,
        gameConfig: gameConfig,
        logger:     logger,
    }
}

func (s *EventService) ProcessEvent(roomID, eventType string, viewerID *string) (*model.EventResult, error) {
    logger := s.logger.With(
        slog.String("service", "event"),
        slog.String("op", "process"),
        slog.String("room_id", roomID),
        slog.String("event_type", eventType))

    if viewerID != nil {
        logger = logger.With(slog.String("viewer_id", *viewerID))
    }

    logger.Debug("Processing event started")

    // 1. イベント記録
    ev := &model.Event{
        RoomID:    roomID,
        EventType: model.EventType(eventType),
        ViewerID:  viewerID,
        Metadata:  "{}",
    }
    if err := s.eventRepo.CreateEvent(ev); err != nil {
        logger.Error("Failed to record event", slog.Any("error", err))
        return nil, fmt.Errorf("record event: %w", err)
    }
    logger.Debug("Event recorded in DB")

    // 2. 視聴者アクティビティ更新
    if viewerID != nil {
        if err := s.counter.UpdateViewerActivity(roomID, *viewerID); err != nil {
            logger.Warn("Failed to update viewer activity", slog.Any("error", err))
        } else {
            logger.Debug("Viewer activity updated")
        }
    }

    // 3. カウント増加
    current, err := s.counter.Increment(roomID, eventType)
    if err != nil {
        logger.Error("Failed to increment counter", slog.Any("error", err))
        return nil, fmt.Errorf("increment counter: %w", err)
    }
    logger.Debug("Counter incremented", slog.Int64("current", current))

    // 4. アクティブ視聴者数取得
    viewers := s.getActiveViewerCount(roomID)
    logger.Debug("Active viewers counted", slog.Int("count", viewers))

    // 5. 閾値計算
    threshold, err := s.gameConfig.CalculateThreshold(eventType, viewers)
    if err != nil {
        logger.Error("Failed to calculate threshold", slog.Any("error", err))
        return nil, fmt.Errorf("calculate threshold: %w", err)
    }
    logger.Debug("Threshold calculated",
        slog.Int("threshold", threshold),
        slog.Int("viewers", viewers))

    res := &model.EventResult{
        EventType:       eventType,
        CurrentCount:    int(current),
        RequiredCount:   threshold,
        ViewerCount:     viewers,
        EffectTriggered: false,
        NextThreshold:   threshold,
    }

    // 6. 閾値判定と発動
    if int(current) >= threshold {
        logger.Info("🎯 Threshold reached - triggering effect",
            slog.Int("current", int(current)),
            slog.Int("threshold", threshold))

        payload := map[string]interface{}{
            "type":          "game_event",
            "event_type":    eventType,
            "trigger_count": int(current),
            "viewer_count":  viewers,
        }

        if err := s.wsHandler.SendEventToUnity(roomID, payload); err != nil {
            logger.Error("Failed to send event to Unity", slog.Any("error", err))
        } else {
            logger.Info("✅ Effect triggered and sent to Unity")
        }

        // カウンタリセット
        if err := s.counter.Reset(roomID, eventType); err != nil {
            logger.Warn("Failed to reset counter", slog.Any("error", err))
        } else {
            logger.Debug("Counter reset")
        }

        res.EffectTriggered = true
        res.CurrentCount = 0

        // 次の閾値を再計算（視聴者数が変動している可能性があるため）
        nextViewers := s.getActiveViewerCount(roomID)
        nextThreshold, _ := s.gameConfig.CalculateThreshold(eventType, nextViewers)
        res.NextThreshold = nextThreshold
        logger.Debug("Next threshold calculated", slog.Int("next", nextThreshold))
    }

    logger.Debug("Event processing completed",
        slog.Bool("triggered", res.EffectTriggered),
        slog.Int("final_count", res.CurrentCount))

    return res, nil
}

func (s *EventService) GetRoomStats(roomID string) ([]model.RoomEventStat, error) {
    logger := s.logger.With(
        slog.String("service", "event"),
        slog.String("op", "get_stats"),
        slog.String("room_id", roomID))

    logger.Debug("Fetching room stats")

    viewers := s.getActiveViewerCount(roomID)
    logger.Debug("Active viewers for stats", slog.Int("count", viewers))

    eventTypes := s.gameConfig.ListEventTypes()
    stats := make([]model.RoomEventStat, 0, len(eventTypes))

    for _, et := range eventTypes {
        current, err := s.counter.Get(roomID, et)
        if err != nil {
            logger.Error("Failed to get counter",
                slog.String("event_type", et),
                slog.Any("error", err))
            return nil, fmt.Errorf("get counter for %s: %w", et, err)
        }

        threshold, err := s.gameConfig.CalculateThreshold(et, viewers)
        if err != nil {
            logger.Error("Failed to calculate threshold",
                slog.String("event_type", et),
                slog.Any("error", err))
            return nil, fmt.Errorf("calculate threshold for %s: %w", et, err)
        }

        stats = append(stats, model.RoomEventStat{
            EventType:     et,
            CurrentCount:  int(current),
            CurrentLevel:  1,
            RequiredCount: threshold,
            NextThreshold: threshold,
            ViewerCount:   viewers,
        })

        logger.Debug("Stat collected",
            slog.String("event_type", et),
            slog.Int("current", int(current)),
            slog.Int("threshold", threshold))
    }

    logger.Info("Room stats compiled", slog.Int("stat_count", len(stats)))
    return stats, nil
}

func (s *EventService) getActiveViewerCount(roomID string) int {
    logger := s.logger.With(
        slog.String("service", "event"),
        slog.String("room_id", roomID))

    count, err := s.counter.GetActiveViewerCount(roomID)
    if err != nil {
        logger.Warn("Failed to get active viewer count, using default",
            slog.Any("error", err))
        return 1
    }

    if count < 1 {
        logger.Debug("No active viewers, using minimum", slog.Int64("raw_count", count))
        return 1
    }

    if count > 1_000_000 {
        logger.Warn("Viewer count exceeds safety limit, clamping",
            slog.Int64("raw_count", count))
        return 1_000_000
    }

    logger.Debug("Active viewer count retrieved", slog.Int("count", int(count)))
    return int(count)
}