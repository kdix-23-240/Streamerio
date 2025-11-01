package repository

import (
	"database/sql"
	"log/slog"
	"time"

	"streamerrio-backend/internal/model"

	"github.com/jmoiron/sqlx"
)

// EventRepository: イベント永続化用インタフェース
// 主要イベントクエリのトレースログを出力し、運用時の観測性を高める。
type EventRepository interface {
	CreateEvent(event *model.Event) error // 単一イベント挿入
	ListEventViewerCounts(roomID string) ([]model.EventAggregate, error)
	ListEventTotals(roomID string) ([]model.EventTotal, error)
	ListViewerTotals(roomID string) ([]model.ViewerTotal, error)
	ListViewerEventCounts(roomID, viewerID string) ([]model.ViewerEventCount, error)
}

type eventRepository struct {
	db     *sqlx.DB
	logger *slog.Logger

	// 準備済みステートメントを保持
	createEventStmt           *sqlx.Stmt
	listEventViewerCountsStmt *sqlx.Stmt
	listEventTotalsStmt       *sqlx.Stmt
	listViewerTotalsStmt      *sqlx.Stmt
	listViewerEventCountsStmt *sqlx.Stmt
}

// NewEventRepository: 実装生成
func NewEventRepository(db *sqlx.DB, logger *slog.Logger) EventRepository {
	if logger == nil {
		logger = slog.Default()
	}

	// 準備に失敗した場合は、起動時エラーとして panic させる
	mustPrepare := func(query string) *sqlx.Stmt {
		stmt, err := db.Preparex(query)
		if err != nil {
			logger.Error("failed to prepare statement", slog.Any("error", err), slog.String("query", query))
			panic(err)
		}
		return stmt
	}

	return &eventRepository{
		db:                        db,
		logger:                    logger,
		createEventStmt:           mustPrepare(queryCreateEvent),
		listEventViewerCountsStmt: mustPrepare(queryListEventViewerCounts),
		listEventTotalsStmt:       mustPrepare(queryListEventTotals),
		listViewerTotalsStmt:      mustPrepare(queryListViewerTotals),
		listViewerEventCountsStmt: mustPrepare(queryListViewerEventCounts),
	}
}

// CreateEvent: events テーブルへ挿入 (TriggeredAt 未設定なら現在時刻)
func (r *eventRepository) CreateEvent(event *model.Event) error {
	if event.TriggeredAt.IsZero() {
		event.TriggeredAt = time.Now()
	}
	attrs := []any{
		slog.String("repo", "event"),
		slog.String("op", "create_event"),
		slog.String("room_id", event.RoomID),
		slog.String("event_type", string(event.EventType)),
		slog.Bool("has_viewer", event.ViewerID != nil),
	}
	if event.ViewerID != nil {
		attrs = append(attrs, slog.String("viewer_id", *event.ViewerID))
	}
	logger := r.logger.With(attrs...)
	start := time.Now()
	res, err := r.createEventStmt.Exec(event.RoomID, event.ViewerID, event.EventType, event.TriggeredAt, event.Metadata)
	if err != nil {
		logger.Error("db.exec (prepared) failed", slog.Any("error", err))
		return err
	}
	rows, _ := res.RowsAffected()
	logger.Debug("db.exec", slog.Int64("rows_affected", rows), slog.Duration("elapsed", time.Since(start)))
	return nil
}

func (r *eventRepository) ListEventViewerCounts(roomID string) ([]model.EventAggregate, error) {
	rows := []struct {
		EventType  model.EventType `db:"event_type"`
		ViewerID   sql.NullString  `db:"viewer_id"`
		ViewerName sql.NullString  `db:"viewer_name"`
		Count      int             `db:"count"`
	}{}
	logger := r.logger.With(
		slog.String("repo", "event"),
		slog.String("op", "list_event_viewer_counts"),
		slog.String("room_id", roomID),
	)
	start := time.Now()
	if err := r.listEventViewerCountsStmt.Select(&rows, roomID); err != nil {
		logger.Error("db.query (prepared) failed", slog.Any("error", err))
		return nil, err
	}
	logger.Debug("db.query", slog.Int("row_count", len(rows)), slog.Duration("elapsed", time.Since(start)))

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
	logger := r.logger.With(
		slog.String("repo", "event"),
		slog.String("op", "list_event_totals"),
		slog.String("room_id", roomID),
	)
	start := time.Now()
	if err := r.listEventTotalsStmt.Select(&rows, roomID); err != nil {
		logger.Error("db.query (prepared) failed", slog.Any("error", err))
		return nil, err
	}
	logger.Debug("db.query", slog.Int("row_count", len(rows)), slog.Duration("elapsed", time.Since(start)))
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
	logger := r.logger.With(
		slog.String("repo", "event"),
		slog.String("op", "list_viewer_totals"),
		slog.String("room_id", roomID),
	)
	start := time.Now()
	if err := r.db.Select(&rows, q, roomID); err != nil {
		logger.Error("db.query failed", slog.Any("error", err))
		return nil, err
	}
	logger.Debug("db.query", slog.Int("row_count", len(rows)), slog.Duration("elapsed", time.Since(start)))

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
	logger := r.logger.With(
		slog.String("repo", "event"),
		slog.String("op", "list_viewer_event_counts"),
		slog.String("room_id", roomID),
		slog.String("viewer_id", viewerID),
	)
	start := time.Now()
	if err := r.db.Select(&rows, q, roomID, viewerID); err != nil {
		logger.Error("db.query failed", slog.Any("error", err))
		return nil, err
	}
	logger.Debug("db.query", slog.Int("row_count", len(rows)), slog.Duration("elapsed", time.Since(start)))
	return rows, nil
}

func cloneString(s string) *string {
	val := s
	return &val
}
