package service

import (
    "fmt"
    "log"
    "math"

    "streamerrio-backend/internal/model"
    "streamerrio-backend/internal/repository"
    "streamerrio-backend/pkg/counter"
)

// WebSocket 送信用インタフェース (Unity へゲームイベント通知するための最小限)
type WebSocketSender interface {
	SendEventToUnity(roomID string, payload map[string]interface{}) error
}

type EventService struct {
	counter   counter.Counter
	eventRepo repository.EventRepository
	wsHandler WebSocketSender
	configs   map[model.EventType]*model.EventConfig
}

// NewEventService: 依存（カウンタ / リポジトリ / WebSocket）を束ねてサービス生成
func NewEventService(counter counter.Counter, eventRepo repository.EventRepository, wsHandler WebSocketSender) *EventService {
	return &EventService{counter: counter, eventRepo: eventRepo, wsHandler: wsHandler, configs: getDefaultEventConfigs()}
}

// ProcessEvent: 1イベント処理の本流 (DB保存→視聴者アクティビティ更新→カウント加算→閾値判定→発動通知/リセット)
func (s *EventService) ProcessEvent(roomID string, eventType model.EventType, viewerID *string) (*model.EventResult, error) {
	// eventType が有効かチェック
	if _, ok := s.configs[eventType]; !ok {
		return nil, fmt.Errorf("invalid event type: %s", eventType)
	}

	// 1. Record event
	ev := &model.Event{RoomID: roomID, EventType: eventType, ViewerID: viewerID, Metadata: "{}"}
	if err := s.eventRepo.CreateEvent(ev); err != nil {
		return nil, fmt.Errorf("record event failed: %w", err)
	}

	// 2. Update viewer activity (backend-agnostic)
	if viewerID != nil {
		_ = s.counter.UpdateViewerActivity(roomID, *viewerID)
	}

	// 3. Increment counter
	current, err := s.counter.Increment(roomID, string(eventType))
	if err != nil {
		return nil, fmt.Errorf("increment failed: %w", err)
	}

	// 4. Active viewer count
	viewers := s.getActiveViewerCount(roomID)

	// 5. Threshold 
	cfg := s.configs[eventType]
	threshold := s.calculateDynamicThreshold(cfg, viewers)

	res := &model.EventResult{EventType: eventType, CurrentCount: int(current), RequiredCount: threshold, ViewerCount: viewers, EffectTriggered: false, NextThreshold: threshold}

	if int(current) >= threshold {
		log.Printf("🚀 trigger room=%s event=%s count=%d threshold=%d viewers=%d", roomID, eventType, current, threshold, viewers)
		payload := map[string]interface{}{
			"type":          "game_event",
			"event_type":    string(eventType),
			"trigger_count": int(current),
			"viewer_count":  viewers,
		}
		if err := s.wsHandler.SendEventToUnity(roomID, payload); err != nil {
			log.Printf("❌ unity send failed: %v", err)
		} else {
			log.Printf("✅ unity notified: room=%s event=%s", roomID, eventType)
		}
		_ = s.counter.Reset(roomID, string(eventType))
		res.EffectTriggered = true
		res.NextThreshold = s.calculateDynamicThreshold(cfg, s.getActiveViewerCount(roomID))
		res.CurrentCount = 0
	}
	return res, nil
}

// calculateDynamicThreshold: 視聴者数に応じた動的閾値を算出し上下限でクランプ
func (s *EventService) calculateDynamicThreshold(cfg *model.EventConfig, viewerCount int) int {
	mult := s.getViewerMultiplier(viewerCount)
	raw := float64(cfg.BaseThreshold) * mult
	val := int(math.Ceil(raw))
	if val < cfg.MinThreshold {
		val = cfg.MinThreshold
	}
	if val > cfg.MaxThreshold {
		val = cfg.MaxThreshold
	}
	return val
}

// getViewerMultiplier: 視聴者数帯ごとの倍率テーブル
func (s *EventService) getViewerMultiplier(v int) float64 {
	switch {
	case v <= 5:
		return 1.0
	case v <= 10:
		return 1.2
	case v <= 20:
		return 1.5
	case v <= 50:
		return 2.0
	default:
		return 3.0
	}
}

// getActiveViewerCount: アクティブ視聴者数取得 (0 やエラー時は 1 にフォールバック)
func (s *EventService) getActiveViewerCount(roomID string) int {
	c, err := s.counter.GetActiveViewerCount(roomID)
	if err != nil || c < 1 {
		return 1
	}
	if c > 1_000_000 { // safety clamp
		return 1_000_000
	}
	return int(c)
}

// Stats (simplified, no level)
// RoomEventStat: 統計表示用の簡易集計構造体
type RoomEventStat struct {
	EventType     model.EventType `json:"event_type"`
	CurrentCount  int             `json:"current_count"`
	CurrentLevel  int             `json:"current_level"` // always 1
	RequiredCount int             `json:"required_count"`
	NextThreshold int             `json:"next_threshold"`
	ViewerCount   int             `json:"viewer_count"`
}

// GetRoomStats: 全イベント種別について現在カウントと閾値をまとめて返却
func (s *EventService) GetRoomStats(roomID string) ([]RoomEventStat, error) {
	viewers := s.getActiveViewerCount(roomID)
	stats := make([]RoomEventStat, 0, len(s.configs))
	for et, cfg := range s.configs {
		cur, err := s.counter.Get(roomID, string(et))
		if err != nil {
			return nil, fmt.Errorf("get counter failed: %w", err)
		}
		th := s.calculateDynamicThreshold(cfg, viewers)
		stats = append(stats, RoomEventStat{EventType: et, CurrentCount: int(cur), CurrentLevel: 1, RequiredCount: th, NextThreshold: th, ViewerCount: viewers})
	}
	return stats, nil
}

// Default configs (unchanged thresholds foundation)
// getDefaultEventConfigs: 初期閾値設定マップ生成
func getDefaultEventConfigs() map[model.EventType]*model.EventConfig {
	return map[model.EventType]*model.EventConfig{
		model.SKILL1:    {EventType: model.SKILL1, BaseThreshold: 5, MinThreshold: 3, MaxThreshold: 50, LevelMultiplier: 1.3},
		model.SKILL2:     {EventType: model.SKILL2, BaseThreshold: 6, MinThreshold: 4, MaxThreshold: 60, LevelMultiplier: 1.3},
		model.SKILL3:     {EventType: model.SKILL3, BaseThreshold: 12, MinThreshold: 8, MaxThreshold: 100, LevelMultiplier: 1.4},
		model.ENEMY1:   {EventType: model.ENEMY1, BaseThreshold: 6, MinThreshold: 4, MaxThreshold: 45, LevelMultiplier: 1.3},
		model.ENEMY2:   {EventType: model.ENEMY2, BaseThreshold: 7, MinThreshold: 5, MaxThreshold: 55, LevelMultiplier: 1.4},
		model.ENEMY3: {EventType: model.ENEMY3, BaseThreshold: 10, MinThreshold: 6, MaxThreshold: 80, LevelMultiplier: 1.5},
	}
}
