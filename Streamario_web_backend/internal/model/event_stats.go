package model

import "time"

type EventAggregate struct {
    EventType  EventType `db:"event_type" json:"event_type"`
    ViewerID   string    `db:"viewer_id" json:"viewer_id"`
    ViewerName *string   `db:"viewer_name" json:"viewer_name"`
    Count      int       `db:"count" json:"count"`
}

type EventTotal struct {
    EventType EventType `db:"event_type" json:"event_type"`
    Count     int       `db:"count" json:"count"`
}

type ViewerTotal struct {
    ViewerID   string  `db:"viewer_id" json:"viewer_id"`
    ViewerName *string `db:"viewer_name" json:"viewer_name"`
    Count      int     `db:"count" json:"count"`
}

type ViewerEventCount struct {
    EventType EventType `db:"event_type" json:"event_type"`
    Count     int       `db:"count" json:"count"`
}

type EventTop struct {
    ViewerID   string  `json:"viewer_id"`
    ViewerName *string `json:"viewer_name"`
    Count      int     `json:"count"`
}

type RoomResultSummary struct {
    RoomID       string                 `json:"room_id"`
    EndedAt      time.Time              `json:"ended_at"`
    TopByEvent   map[EventType]EventTop `json:"top_by_event"`
    TopOverall   *EventTop              `json:"top_overall,omitempty"`
    EventTotals  map[EventType]int      `json:"event_totals"`
    ViewerTotals []ViewerTotal          `json:"viewer_totals"`
}

type TeamTopSummary struct {
    TopSkill *EventTop `json:"top_skill"`
    TopEnemy *EventTop `json:"top_enemy"`
    TopAll   *EventTop `json:"top_all"`
}

type ViewerSummary struct {
    ViewerID   string            `json:"viewer_id"`
    ViewerName *string           `json:"viewer_name"`
    Counts     map[EventType]int `json:"counts"`
    Total      int               `json:"total"`
}