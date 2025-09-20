package repository

import (
	"database/sql"
	"streamerrio-backend/internal/model"
	"time"

	"github.com/jmoiron/sqlx"
)

// EventRepository: イベント永続化用インタフェース
type EventRepository interface {
	CreateEvent(event *model.Event) error // 単一イベント挿入
	ListEventViewerCounts(roomID string) ([]model.EventAggregate, error)
	ListEventTotals(roomID string) ([]model.EventTotal, error)
	ListViewerTotals(roomID string) ([]model.ViewerTotal, error)
	ListViewerEventCounts(roomID, viewerID string) ([]model.ViewerEventCount, error)
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

func (r *eventRepository) ListEventViewerCounts(roomID string) ([]model.EventAggregate, error) {
	rows := []struct {
		EventType  model.EventType `db:"event_type"`
		ViewerID   sql.NullString  `db:"viewer_id"`
		ViewerName sql.NullString  `db:"viewer_name"`
		Count      int             `db:"count"`
	}{}
	q := `SELECT e.event_type,
	             e.viewer_id,
	             v.name AS viewer_name,
	             COUNT(*) AS count
	      FROM events e
	      LEFT JOIN viewers v ON v.id = e.viewer_id
	      WHERE e.room_id = $1 AND e.viewer_id IS NOT NULL
	      GROUP BY e.event_type, e.viewer_id, v.name`
	if err := r.db.Select(&rows, q, roomID); err != nil {
		return nil, err
	}
	aggs := make([]model.EventAggregate, 0, len(rows))
	for _, row := range rows {
		if !row.ViewerID.Valid {
			continue
		}
		var namePtr *string
		if row.ViewerName.Valid {
			namePtr = cloneString(row.ViewerName.String)
		}
		aggs = append(aggs, model.EventAggregate{EventType: row.EventType, ViewerID: row.ViewerID.String, ViewerName: namePtr, Count: row.Count})
	}
	return aggs, nil
}

func (r *eventRepository) ListEventTotals(roomID string) ([]model.EventTotal, error) {
	rows := []model.EventTotal{}
	q := `SELECT event_type, COUNT(*) AS count
		FROM events
		WHERE room_id = $1
		GROUP BY event_type`
	if err := r.db.Select(&rows, q, roomID); err != nil {
		return nil, err
	}
	return rows, nil
}

func (r *eventRepository) ListViewerTotals(roomID string) ([]model.ViewerTotal, error) {
	rows := []struct {
		ViewerID   sql.NullString `db:"viewer_id"`
		ViewerName sql.NullString `db:"viewer_name"`
		Count      int            `db:"count"`
	}{}
	q := `SELECT e.viewer_id,
	             v.name AS viewer_name,
	             COUNT(*) AS count
	      FROM events e
	      LEFT JOIN viewers v ON v.id = e.viewer_id
	      WHERE e.room_id = $1 AND e.viewer_id IS NOT NULL
	      GROUP BY e.viewer_id, v.name
	      ORDER BY count DESC, e.viewer_id`
	if err := r.db.Select(&rows, q, roomID); err != nil {
		return nil, err
	}
	totals := make([]model.ViewerTotal, 0, len(rows))
	for _, row := range rows {
		if !row.ViewerID.Valid || row.ViewerID.String == "" {
			continue
		}
		var namePtr *string
		if row.ViewerName.Valid {
			namePtr = cloneString(row.ViewerName.String)
		}
		totals = append(totals, model.ViewerTotal{ViewerID: row.ViewerID.String, ViewerName: namePtr, Count: row.Count})
	}
	return totals, nil
}

func (r *eventRepository) ListViewerEventCounts(roomID, viewerID string) ([]model.ViewerEventCount, error) {
	rows := []model.ViewerEventCount{}
	q := `SELECT event_type, COUNT(*) AS count
		FROM events
		WHERE room_id = $1 AND viewer_id = $2
		GROUP BY event_type`
	if err := r.db.Select(&rows, q, roomID, viewerID); err != nil {
		return nil, err
	}
	return rows, nil
}

func cloneString(s string) *string {
	val := s
	return &val
}
