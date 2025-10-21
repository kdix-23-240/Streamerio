package service

import (
    "errors"
    "fmt"
    "log/slog"
    "time"

    "streamerrio-backend/internal/model"
    "streamerrio-backend/internal/repository"
    "streamerrio-backend/pkg/counter"
)

type GameSessionService struct {
    roomService *RoomService
    eventRepo   repository.EventRepository
    viewerRepo  repository.ViewerRepository
    counter     counter.Counter
    wsSender    WebSocketSender
    logger      *slog.Logger
}

func NewGameSessionService(
    roomService *RoomService,
    eventRepo repository.EventRepository,
    viewerRepo repository.ViewerRepository,
    counter counter.Counter,
    sender WebSocketSender,
    logger *slog.Logger,
) *GameSessionService {
    if logger == nil {
        logger = slog.Default()
    }
    return &GameSessionService{
        roomService: roomService,
        eventRepo:   eventRepo,
        viewerRepo:  viewerRepo,
        counter:     counter,
        wsSender:    sender,
        logger:      logger,
    }
}

func (s *GameSessionService) EndGame(roomID string) (*model.RoomResultSummary, error) {
    logger := s.logger.With(
        slog.String("service", "session"),
        slog.String("op", "end_game"),
        slog.String("room_id", roomID))

    logger.Info("Ending game session")

    room, err := s.roomService.GetRoom(roomID)
    if err != nil {
        logger.Error("Failed to get room", slog.Any("error", err))
        return nil, fmt.Errorf("get room: %w", err)
    }

    if room == nil {
        logger.Warn("Room not found")
        return nil, errors.New("room not found")
    }

    if room.Status == "ended" {
        logger.Info("Room already ended, returning existing results")
        return s.GetRoomResult(roomID)
    }

    logger.Debug("Building room summary")
    summary, err := s.buildRoomSummary(roomID)
    if err != nil {
        logger.Error("Failed to build summary", slog.Any("error", err))
        return nil, fmt.Errorf("build summary: %w", err)
    }

    endedAt := time.Now()
    logger.Debug("Marking room as ended", slog.Time("ended_at", endedAt))

    if err := s.roomService.MarkEnded(roomID, endedAt); err != nil {
        logger.Error("Failed to mark room as ended", slog.Any("error", err))
        return nil, fmt.Errorf("mark ended: %w", err)
    }

    summary.RoomID = roomID
    summary.EndedAt = endedAt

    // Redisカウンタリセット
    logger.Debug("Resetting Redis counters")
    for _, et := range model.ListEventTypes() {
        if err := s.counter.Reset(roomID, string(et)); err != nil {
            logger.Warn("Failed to reset counter",
                slog.String("event_type", string(et)),
                slog.Any("error", err))
        }
    }

    // Unity へ終了サマリー送信
    if s.wsSender != nil {
        logger.Debug("Sending end summary to Unity")
        teamTops := s.buildTeamTop(summary)
        payload := map[string]interface{}{
            "type":          "game_end_summary",
            "top_by_button": summary.TopByEvent,
            "top_overall":   summary.TopOverall,
            "team_tops": map[string]interface{}{
                "skill": s.eventTopToPayload(teamTops.TopSkill),
                "enemy": s.eventTopToPayload(teamTops.TopEnemy),
                "all":   s.eventTopToPayload(teamTops.TopAll),
            },
        }
        if err := s.wsSender.SendEventToUnity(roomID, payload); err != nil {
            logger.Warn("Failed to send end summary to Unity", slog.Any("error", err))
        } else {
            logger.Info("End summary sent to Unity successfully")
        }
    }

    logger.Info("Game session ended successfully",
        slog.Int("total_viewers", len(summary.ViewerTotals)))

    return summary, nil
}

func (s *GameSessionService) GetRoomResult(roomID string) (*model.RoomResultSummary, error) {
    logger := s.logger.With(
        slog.String("service", "session"),
        slog.String("op", "get_result"),
        slog.String("room_id", roomID))

    logger.Debug("Fetching room result")

    room, err := s.roomService.GetRoom(roomID)
    if err != nil {
        logger.Error("Failed to get room", slog.Any("error", err))
        return nil, fmt.Errorf("get room: %w", err)
    }

    if room == nil {
        logger.Warn("Room not found")
        return nil, errors.New("room not found")
    }

    summary, err := s.buildRoomSummary(roomID)
    if err != nil {
        logger.Error("Failed to build summary", slog.Any("error", err))
        return nil, fmt.Errorf("build summary: %w", err)
    }

    summary.RoomID = roomID
    if room.EndedAt != nil {
        summary.EndedAt = *room.EndedAt
    } else {
        summary.EndedAt = time.Now()
    }

    logger.Info("Room result retrieved successfully")
    return summary, nil
}

func (s *GameSessionService) GetViewerSummary(roomID, viewerID string) (*model.ViewerSummary, error) {
    logger := s.logger.With(
        slog.String("service", "session"),
        slog.String("op", "get_viewer_summary"),
        slog.String("room_id", roomID),
        slog.String("viewer_id", viewerID))

    logger.Debug("Fetching viewer summary")

    if viewerID == "" {
        logger.Warn("Viewer ID required")
        return nil, fmt.Errorf("viewer_id required")
    }

    rows, err := s.eventRepo.ListViewerEventCounts(roomID, viewerID)
    if err != nil {
        logger.Error("Failed to list viewer event counts", slog.Any("error", err))
        return nil, fmt.Errorf("list viewer event counts: %w", err)
    }

    counts := make(map[model.EventType]int, len(rows))
    total := 0
    for _, row := range rows {
        counts[row.EventType] = row.Count
        total += row.Count
    }

    // すべてのイベントタイプを0で初期化
    for _, et := range model.ListEventTypes() {
        if _, ok := counts[et]; !ok {
            counts[et] = 0
        }
    }

    var namePtr *string
    if s.viewerRepo != nil {
        if viewer, err := s.viewerRepo.Get(viewerID); err == nil && viewer != nil && viewer.Name != nil {
            namePtr = cloneStringPointer(viewer.Name)
            logger.Debug("Viewer name retrieved", slog.String("name", *namePtr))
        }
    }

    logger.Info("Viewer summary compiled",
        slog.Int("total_events", total),
        slog.Int("event_types", len(counts)))

    return &model.ViewerSummary{
        ViewerID:   viewerID,
        ViewerName: namePtr,
        Counts:     counts,
        Total:      total,
    }, nil
}

func (s *GameSessionService) buildRoomSummary(roomID string) (*model.RoomResultSummary, error) {
    logger := s.logger.With(
        slog.String("service", "session"),
        slog.String("op", "build_summary"),
        slog.String("room_id", roomID))

    logger.Debug("Building room summary")

    // イベント×視聴者集計
    aggs, err := s.eventRepo.ListEventViewerCounts(roomID)
    if err != nil {
        logger.Error("Failed to list event viewer counts", slog.Any("error", err))
        return nil, fmt.Errorf("list event viewer counts: %w", err)
    }
    logger.Debug("Event viewer counts fetched", slog.Int("count", len(aggs)))

    // イベント合計
    eventTotals, err := s.eventRepo.ListEventTotals(roomID)
    if err != nil {
        logger.Error("Failed to list event totals", slog.Any("error", err))
        return nil, fmt.Errorf("list event totals: %w", err)
    }
    logger.Debug("Event totals fetched", slog.Int("count", len(eventTotals)))

    // 視聴者合計
    viewerTotals, err := s.eventRepo.ListViewerTotals(roomID)
    if err != nil {
        logger.Error("Failed to list viewer totals", slog.Any("error", err))
        return nil, fmt.Errorf("list viewer totals: %w", err)
    }
    logger.Debug("Viewer totals fetched", slog.Int("count", len(viewerTotals)))

    // イベント種別ごとのトップ算出
    topByEvent := make(map[model.EventType]model.EventTop, len(model.ListEventTypes()))
    for _, et := range model.ListEventTypes() {
        topByEvent[et] = model.EventTop{ViewerID: "", ViewerName: nil, Count: 0}
    }

    var topOverall *model.EventTop
    for _, agg := range aggs {
        if agg.ViewerID == "" {
            continue
        }

        current := topByEvent[agg.EventType]
        if agg.Count > current.Count || (agg.Count == current.Count && (current.ViewerID == "" || agg.ViewerID < current.ViewerID)) {
            topByEvent[agg.EventType] = model.EventTop{
                ViewerID:   agg.ViewerID,
                ViewerName: cloneStringPointer(agg.ViewerName),
                Count:      agg.Count,
            }
        }

        if topOverall == nil || agg.Count > topOverall.Count || (agg.Count == topOverall.Count && (topOverall.ViewerID == "" || agg.ViewerID < topOverall.ViewerID)) {
            tmp := model.EventTop{
                ViewerID:   agg.ViewerID,
                ViewerName: cloneStringPointer(agg.ViewerName),
                Count:      agg.Count,
            }
            topOverall = &tmp
        }
    }

    totalMap := make(map[model.EventType]int, len(model.ListEventTypes()))
    for _, et := range model.ListEventTypes() {
        totalMap[et] = 0
    }
    for _, total := range eventTotals {
        totalMap[total.EventType] = total.Count
    }

    logger.Info("Room summary built successfully",
        slog.Int("top_by_event_count", len(topByEvent)),
        slog.Int("total_viewers", len(viewerTotals)))

    return &model.RoomResultSummary{
        RoomID:       roomID,
        TopByEvent:   topByEvent,
        TopOverall:   topOverall,
        EventTotals:  totalMap,
        ViewerTotals: viewerTotals,
    }, nil
}

func (s *GameSessionService) buildTeamTop(summary *model.RoomResultSummary) model.TeamTopSummary {
    logger := s.logger.With(
        slog.String("service", "session"),
        slog.String("op", "build_team_top"))

    logger.Debug("Building team tops")

    fetch := func(filter func(model.EventType) bool) *model.EventTop {
        var best *model.EventTop
        for et, top := range summary.TopByEvent {
            if !filter(et) || top.ViewerID == "" {
                continue
            }
            if best == nil || top.Count > best.Count || (top.Count == best.Count && top.ViewerID < best.ViewerID) {
                tmp := top
                best = &tmp
            }
        }
        return best
    }

    teamTop := model.TeamTopSummary{
        TopSkill: fetch(func(et model.EventType) bool {
            return et == model.SKILL1 || et == model.SKILL2 || et == model.SKILL3
        }),
        TopEnemy: fetch(func(et model.EventType) bool {
            return et == model.ENEMY1 || et == model.ENEMY2 || et == model.ENEMY3
        }),
        TopAll: summary.TopOverall,
    }

    logger.Debug("Team tops built")
    return teamTop
}

func (s *GameSessionService) eventTopToPayload(top *model.EventTop) map[string]interface{} {
    if top == nil {
        return nil
    }
    return map[string]interface{}{
        "viewer_id":   top.ViewerID,
        "viewer_name": top.ViewerName,
        "count":       top.Count,
    }
}

func cloneStringPointer(src *string) *string {
    if src == nil {
        return nil
    }
    val := *src
    return &val
}