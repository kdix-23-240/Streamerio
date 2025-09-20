package model

import "time"

// EventAggregate: event_type × viewer_id ごとの集計値
type EventAggregate struct {
	EventType  EventType `db:"event_type" json:"event_type"`
	ViewerID   string    `db:"viewer_id" json:"viewer_id"`
	ViewerName *string   `db:"viewer_name" json:"viewer_name"`
	Count      int       `db:"count" json:"count"`
}

// EventTotal: イベント種別ごとの総押下数
type EventTotal struct {
	EventType EventType `db:"event_type" json:"event_type"`
	Count     int       `db:"count" json:"count"`
}

// ViewerTotal: 視聴者ごとの総押下数
type ViewerTotal struct {
	ViewerID   string  `db:"viewer_id" json:"viewer_id"`
	ViewerName *string `db:"viewer_name" json:"viewer_name"`
	Count      int     `db:"count" json:"count"`
}

// ViewerEventCount: 視聴者がイベント種別ごとに押した回数
type ViewerEventCount struct {
	EventType EventType `db:"event_type" json:"event_type"`
	Count     int       `db:"count" json:"count"`
}

// EventTop: 最多押下者情報
type EventTop struct {
	ViewerID   string  `json:"viewer_id"`
	ViewerName *string `json:"viewer_name"`
	Count      int     `json:"count"`
}

// RoomResultSummary: 結果画面/API 用の集計サマリー
type RoomResultSummary struct {
	RoomID       string                 `json:"room_id"`
	EndedAt      time.Time              `json:"ended_at"`
	TopByEvent   map[EventType]EventTop `json:"top_by_event"`
	TopOverall   *EventTop              `json:"top_overall,omitempty"`
	EventTotals  map[EventType]int      `json:"event_totals"`
	ViewerTotals []ViewerTotal          `json:"viewer_totals"`
}

// ViewerSummary: 終了後に返す視聴者別内訳
type ViewerSummary struct {
	ViewerID   string            `json:"viewer_id"`
	ViewerName *string           `json:"viewer_name"`
	Counts     map[EventType]int `json:"counts"`
	Total      int               `json:"total"`
}
