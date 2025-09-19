package model

import "time"

type EventType string

const (
	HELP_SPEED    EventType = "help_speed"
	HELP_JUMP     EventType = "help_jump"
	HELP_HEAL     EventType = "help_heal"
	HINDER_SLOW   EventType = "hinder_slow"
	HINDER_SLIP   EventType = "hinder_slip"
	HINDER_DAMAGE EventType = "hinder_damage"
)

type Event struct {
	ID          int64     `json:"id" db:"id"`
	RoomID      string    `json:"room_id" db:"room_id"`
	ViewerID    *string   `json:"viewer_id" db:"viewer_id"`
	EventType   EventType `json:"event_type" db:"event_type"`
	TriggeredAt time.Time `json:"triggered_at" db:"triggered_at"`
	Metadata    string    `json:"metadata" db:"metadata"`
}

type EventConfig struct {
	EventType       EventType `json:"event_type"`
	BaseThreshold   int       `json:"base_threshold"`
	MinThreshold    int       `json:"min_threshold"`
	MaxThreshold    int       `json:"max_threshold"`
	LevelMultiplier float64   `json:"level_multiplier"` // 互換性のため残すが未使用
}

type EventResult struct {
	EventType       EventType `json:"event_type"`
	CurrentCount    int       `json:"current_count"`
	RequiredCount   int       `json:"required_count"`
	EffectTriggered bool      `json:"effect_triggered"`
	ViewerCount     int       `json:"viewer_count"`
	NextThreshold   int       `json:"next_threshold"`
}

type RoomEventStat struct {
	EventType     EventType `json:"event_type"`
	CurrentCount  int       `json:"current_count"`
	CurrentLevel  int       `json:"current_level"`
	RequiredCount int       `json:"required_count"`
	NextThreshold int       `json:"next_threshold"`
	ViewerCount   int       `json:"viewer_count"`
}
