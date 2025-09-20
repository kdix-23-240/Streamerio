package service

import (
	"errors"
	"fmt"
	"log"
	"time"

	"streamerrio-backend/internal/model"
	"streamerrio-backend/internal/repository"
	"streamerrio-backend/pkg/counter"
)

// GameSessionService: ゲーム開始〜終了の境界を跨ぐ処理を担当
type GameSessionService struct {
	roomService *RoomService
	eventRepo   repository.EventRepository
	viewerRepo  repository.ViewerRepository
	counter     counter.Counter
	wsSender    WebSocketSender
}

func NewGameSessionService(roomService *RoomService, eventRepo repository.EventRepository, viewerRepo repository.ViewerRepository, counter counter.Counter, sender WebSocketSender) *GameSessionService {
	return &GameSessionService{roomService: roomService, eventRepo: eventRepo, viewerRepo: viewerRepo, counter: counter, wsSender: sender}
}

// EndGame: Unity からの終了通知時に呼ぶ。集計→ルーム終了→Unity へ結果送信までを担う。
func (s *GameSessionService) EndGame(roomID string) (*model.RoomResultSummary, error) {
	room, err := s.roomService.GetRoom(roomID)
	if err != nil {
		return nil, err
	}
	if room == nil {
		return nil, errors.New("room not found")
	}
	if room.Status == "ended" {
		return s.GetRoomResult(roomID)
	}

	summary, err := s.buildRoomSummary(roomID)
	if err != nil {
		return nil, err
	}
	endedAt := time.Now()
	if err := s.roomService.MarkEnded(roomID, endedAt); err != nil {
		return nil, err
	}
	summary.RoomID = roomID
	summary.EndedAt = endedAt

	// Redis カウンタは終了時にリセットしておく（失敗しても致命的ではないためログのみ）
	for _, et := range model.ListEventTypes() {
		if err := s.counter.Reset(roomID, string(et)); err != nil {
			log.Printf("warn: reset counter failed room=%s event=%s err=%v", roomID, et, err)
		}
	}

	// Unity へ終了サマリーを送信
	if s.wsSender != nil {
		payload := map[string]interface{}{
			"type":          "game_end_summary",
			"top_by_button": summary.TopByEvent,
			"top_overall":   summary.TopOverall,
		}
		if err := s.wsSender.SendEventToUnity(roomID, payload); err != nil {
			log.Printf("warn: failed to send end summary to unity room=%s err=%v", roomID, err)
		}
	}

	return summary, nil
}

// GetRoomResult: 終了済みルームの集計結果を取得
func (s *GameSessionService) GetRoomResult(roomID string) (*model.RoomResultSummary, error) {
	room, err := s.roomService.GetRoom(roomID)
	if err != nil {
		return nil, err
	}
	if room == nil {
		return nil, errors.New("room not found")
	}
	summary, err := s.buildRoomSummary(roomID)
	if err != nil {
		return nil, err
	}
	summary.RoomID = roomID
	if room.EndedAt != nil {
		summary.EndedAt = *room.EndedAt
	} else {
		summary.EndedAt = time.Now()
	}
	return summary, nil
}

// GetViewerSummary: 終了後に視聴者へ返す個別内訳
func (s *GameSessionService) GetViewerSummary(roomID, viewerID string) (*model.ViewerSummary, error) {
	if viewerID == "" {
		return nil, fmt.Errorf("viewer_id required")
	}
	rows, err := s.eventRepo.ListViewerEventCounts(roomID, viewerID)
	if err != nil {
		return nil, err
	}
	counts := make(map[model.EventType]int, len(rows))
	total := 0
	for _, row := range rows {
		counts[row.EventType] = row.Count
		total += row.Count
	}
	for _, et := range model.ListEventTypes() {
		if _, ok := counts[et]; !ok {
			counts[et] = 0
		}
	}
	var namePtr *string
	if s.viewerRepo != nil {
		if viewer, err := s.viewerRepo.Get(viewerID); err == nil && viewer != nil && viewer.Name != nil {
			namePtr = cloneStringPointer(viewer.Name)
		}
	}
	return &model.ViewerSummary{ViewerID: viewerID, ViewerName: namePtr, Counts: counts, Total: total}, nil
}

// buildRoomSummary: DB の events をもとに終了サマリーを構築（EndedAt は呼び出し側で設定）
func (s *GameSessionService) buildRoomSummary(roomID string) (*model.RoomResultSummary, error) {
	aggs, err := s.eventRepo.ListEventViewerCounts(roomID)
	if err != nil {
		return nil, err
	}
	eventTotals, err := s.eventRepo.ListEventTotals(roomID)
	if err != nil {
		return nil, err
	}
	viewerTotals, err := s.eventRepo.ListViewerTotals(roomID)
	if err != nil {
		return nil, err
	}

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
			topByEvent[agg.EventType] = model.EventTop{ViewerID: agg.ViewerID, ViewerName: cloneStringPointer(agg.ViewerName), Count: agg.Count}
		}
		if topOverall == nil || agg.Count > topOverall.Count || (agg.Count == topOverall.Count && (topOverall.ViewerID == "" || agg.ViewerID < topOverall.ViewerID)) {
			tmp := model.EventTop{ViewerID: agg.ViewerID, ViewerName: cloneStringPointer(agg.ViewerName), Count: agg.Count}
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

	return &model.RoomResultSummary{
		RoomID:       roomID,
		TopByEvent:   topByEvent,
		TopOverall:   topOverall,
		EventTotals:  totalMap,
		ViewerTotals: viewerTotals,
	}, nil
}

func cloneStringPointer(src *string) *string {
	if src == nil {
		return nil
	}
	val := *src
	return &val
}
