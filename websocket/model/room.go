package model

import "time"


type Room struct {
    ID         string    `json:"id"`
    StreamerID string    `json:"streamer_id"`
    CreatedAt  time.Time `json:"created_at"`
    ExpiresAt  *time.Time `json:"expires_at"`
    Status     string    `json:"status"`
    Settings   string    `json:"settings"`
}

type Event struct {
    ID          int64     `json:"id" db:"id"`
    RoomID      string    `json:"room_id" db:"room_id"`
    ViewerID    *string   `json:"viewer_id" db:"viewer_id"`
    EventType   string    `json:"event_type" db:"event_type"`
    TriggeredAt time.Time `json:"triggered_at" db:"triggered_at"`
    Metadata    string    `json:"metadata" db:"metadata"`
}

type GameEvent struct {
    RoomID      string `json:"room_id"`
    EventType   string `json:"event_type"`
    TriggerCount int   `json:"trigger_count"`
}