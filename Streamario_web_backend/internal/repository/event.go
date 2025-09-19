package repository

import (
	"streamerrio-backend/internal/model"
	"time"

	"github.com/jmoiron/sqlx"
)

// EventRepository: イベント永続化用インタフェース
type EventRepository interface {
	CreateEvent(event *model.Event) error // 単一イベント挿入
}

type eventRepository struct{ db *sqlx.DB }

// NewEventRepository: 実装生成
func NewEventRepository(db *sqlx.DB) EventRepository { return &eventRepository{db: db} }

// CreateEvent: events テーブルへ挿入 (TriggeredAt 未設定なら現在時刻)
func (r *eventRepository) CreateEvent(event *model.Event) error {
	if event.TriggeredAt.IsZero() {
		event.TriggeredAt = time.Now()
	}
	q := `INSERT INTO events (room_id, viewer_id, event_type, triggered_at, metadata) VALUES ($1,$2,$3,$4,$5)`
	_, err := r.db.Exec(q, event.RoomID, event.ViewerID, event.EventType, event.TriggeredAt, event.Metadata)
	return err
}
