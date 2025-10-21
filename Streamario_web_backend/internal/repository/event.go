package repository

import (
    "fmt"
    "log/slog"
    "time"

    "streamerrio-backend/internal/model"

    "github.com/jmoiron/sqlx"
)

type EventRepository interface {
    CreateEvent(event *model.Event) error
    ListEventViewerCounts(roomID string) ([]model.EventAggregate, error)
    ListEventTotals(roomID string) ([]model.EventTotal, error)
    ListViewerTotals(roomID string) ([]model.ViewerTotal, error)
    ListViewerEventCounts(roomID, viewerID string) ([]model.ViewerEventCount, error)
}

type eventRepository struct {
    db     *sqlx.DB
    logger *slog.Logger
}

func NewEventRepository(db *sqlx.DB, logger *slog.Logger) EventRepository {
    if logger == nil {
        logger = slog.Default()
    }
    return &eventRepository{db: db, logger: logger}
}

func (r *eventRepository) CreateEvent(event *model.Event) error {
    if event.TriggeredAt.IsZero() {
        event.TriggeredAt = time.Now()
    }

    logger := r.logger.With(
        slog.String("repo", "event"),
        slog.String("op", "create"),
        slog.String("room_id", event.RoomID),
        slog.String("event_type", string(event.EventType)))

    q := `INSERT INTO events (room_id, viewer_id, event_type, triggered_at, metadata) 
          VALUES ($1, $2, $3, $4, $5)`

    start := time.Now()
    _, err := r.db.Exec(q, event.RoomID, event.ViewerID, event.EventType, event.TriggeredAt, event.Metadata)
    elapsed := time.Since(start)

    if err != nil {
        logger.Error("Failed to create event", slog.Any("error", err), slog.Duration("elapsed", elapsed))
        return fmt.Errorf("create event: %w", err)
    }

    logger.Debug("Event created", slog.Duration("elapsed", elapsed))
    return nil
}

func (r *eventRepository) ListEventViewerCounts(roomID string) ([]model.EventAggregate, error) {
    logger := r.logger.With(
        slog.String("repo", "event"),
        slog.String("op", "list_event_viewer_counts"),
        slog.String("room_id", roomID))

    q := `SELECT e.event_type, e.viewer_id, v.name as viewer_name, COUNT(*) as count
          FROM events e
          LEFT JOIN viewers v ON e.viewer_id = v.id
          WHERE e.room_id = $1 AND e.viewer_id IS NOT NULL
          GROUP BY e.event_type, e.viewer_id, v.name
          ORDER BY count DESC`

    var results []model.EventAggregate
    start := time.Now()
    err := r.db.Select(&results, q, roomID)
    elapsed := time.Since(start)

    if err != nil {
        logger.Error("Failed to list event viewer counts", slog.Any("error", err), slog.Duration("elapsed", elapsed))
        return nil, fmt.Errorf("list event viewer counts: %w", err)
    }

    logger.Debug("Event viewer counts listed",
        slog.Int("count", len(results)),
        slog.Duration("elapsed", elapsed))

    return results, nil
}

func (r *eventRepository) ListEventTotals(roomID string) ([]model.EventTotal, error) {
    logger := r.logger.With(
        slog.String("repo", "event"),
        slog.String("op", "list_event_totals"),
        slog.String("room_id", roomID))

    q := `SELECT event_type, COUNT(*) as count
          FROM events
          WHERE room_id = $1
          GROUP BY event_type`

    var results []model.EventTotal
    start := time.Now()
    err := r.db.Select(&results, q, roomID)
    elapsed := time.Since(start)

    if err != nil {
        logger.Error("Failed to list event totals", slog.Any("error", err), slog.Duration("elapsed", elapsed))
        return nil, fmt.Errorf("list event totals: %w", err)
    }

    logger.Debug("Event totals listed",
        slog.Int("count", len(results)),
        slog.Duration("elapsed", elapsed))

    return results, nil
}

func (r *eventRepository) ListViewerTotals(roomID string) ([]model.ViewerTotal, error) {
    logger := r.logger.With(
        slog.String("repo", "event"),
        slog.String("op", "list_viewer_totals"),
        slog.String("room_id", roomID))

    q := `SELECT e.viewer_id, v.name as viewer_name, COUNT(*) as count
          FROM events e
          LEFT JOIN viewers v ON e.viewer_id = v.id
          WHERE e.room_id = $1 AND e.viewer_id IS NOT NULL
          GROUP BY e.viewer_id, v.name
          ORDER BY count DESC`

    var results []model.ViewerTotal
    start := time.Now()
    err := r.db.Select(&results, q, roomID)
    elapsed := time.Since(start)

    if err != nil {
        logger.Error("Failed to list viewer totals", slog.Any("error", err), slog.Duration("elapsed", elapsed))
        return nil, fmt.Errorf("list viewer totals: %w", err)
    }

    logger.Debug("Viewer totals listed",
        slog.Int("count", len(results)),
        slog.Duration("elapsed", elapsed))

    return results, nil
}

func (r *eventRepository) ListViewerEventCounts(roomID, viewerID string) ([]model.ViewerEventCount, error) {
    logger := r.logger.With(
        slog.String("repo", "event"),
        slog.String("op", "list_viewer_event_counts"),
        slog.String("room_id", roomID),
        slog.String("viewer_id", viewerID))

    q := `SELECT event_type, COUNT(*) as count
          FROM events
          WHERE room_id = $1 AND viewer_id = $2
          GROUP BY event_type`

    var results []model.ViewerEventCount
    start := time.Now()
    err := r.db.Select(&results, q, roomID, viewerID)
    elapsed := time.Since(start)

    if err != nil {
        logger.Error("Failed to list viewer event counts", slog.Any("error", err), slog.Duration("elapsed", elapsed))
        return nil, fmt.Errorf("list viewer event counts: %w", err)
    }

    logger.Debug("Viewer event counts listed",
        slog.Int("count", len(results)),
        slog.Duration("elapsed", elapsed))

    return results, nil
}