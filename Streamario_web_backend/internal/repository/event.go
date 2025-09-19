package repository

import (
	"streamerrio-backend/internal/model"
	"time"

	"github.com/jmoiron/sqlx"
)

type EventRepository interface {
	CreateEvent(event *model.Event) error
}

type eventRepository struct{ db *sqlx.DB }

func NewEventRepository(db *sqlx.DB) EventRepository { return &eventRepository{db: db} }

func (r *eventRepository) CreateEvent(event *model.Event) error {
	if event.TriggeredAt.IsZero() {
		event.TriggeredAt = time.Now()
	}
	q := `INSERT INTO events (room_id, viewer_id, event_type, triggered_at, metadata) VALUES ($1,$2,$3,$4,$5)`
	_, err := r.db.Exec(q, event.RoomID, event.ViewerID, event.EventType, event.TriggeredAt, event.Metadata)
	return err
}
