package repository

import (
    "database/sql"
    "fmt"
    "log/slog"
    "time"

    "streamerrio-backend/internal/model"

    "github.com/jmoiron/sqlx"
)

type RoomRepository interface {
    Create(room *model.Room) error
    Get(id string) (*model.Room, error)
    UpdateStatus(id, status string) error
    MarkEnded(id string, endedAt time.Time) error
}

type roomRepository struct {
    db     *sqlx.DB
    logger *slog.Logger
}

func NewRoomRepository(db *sqlx.DB, logger *slog.Logger) RoomRepository {
    if logger == nil {
        logger = slog.Default()
    }
    return &roomRepository{db: db, logger: logger}
}

func (r *roomRepository) Create(room *model.Room) error {
    if room.CreatedAt.IsZero() {
        room.CreatedAt = time.Now()
    }

    logger := r.logger.With(
        slog.String("repo", "room"),
        slog.String("op", "create"),
        slog.String("room_id", room.ID))

    q := `INSERT INTO rooms (id, streamer_id, created_at, expires_at, status, settings)
          VALUES ($1, $2, $3, $4, $5, $6)`

    start := time.Now()
    _, err := r.db.Exec(q, room.ID, room.StreamerID, room.CreatedAt, room.ExpiresAt, room.Status, room.Settings)
    elapsed := time.Since(start)

    if err != nil {
        logger.Error("Failed to create room", slog.Any("error", err), slog.Duration("elapsed", elapsed))
        return fmt.Errorf("create room: %w", err)
    }

    logger.Info("Room created",
        slog.String("streamer_id", room.StreamerID),
        slog.Duration("elapsed", elapsed))

    return nil
}

func (r *roomRepository) Get(id string) (*model.Room, error) {
    logger := r.logger.With(
        slog.String("repo", "room"),
        slog.String("op", "get"),
        slog.String("room_id", id))

    var room model.Room
    q := `SELECT id, streamer_id, created_at, expires_at, status, settings, ended_at
          FROM rooms WHERE id = $1`

    start := time.Now()
    err := r.db.Get(&room, q, id)
    elapsed := time.Since(start)

    if err != nil {
        if err == sql.ErrNoRows {
            logger.Debug("Room not found", slog.Duration("elapsed", elapsed))
            return nil, nil
        }
        logger.Error("Failed to get room", slog.Any("error", err), slog.Duration("elapsed", elapsed))
        return nil, fmt.Errorf("get room: %w", err)
    }

    logger.Debug("Room retrieved",
        slog.String("status", room.Status),
        slog.Duration("elapsed", elapsed))

    return &room, nil
}

func (r *roomRepository) UpdateStatus(id, status string) error {
    logger := r.logger.With(
        slog.String("repo", "room"),
        slog.String("op", "update_status"),
        slog.String("room_id", id),
        slog.String("status", status))

    q := `UPDATE rooms SET status = $1 WHERE id = $2`

    start := time.Now()
    res, err := r.db.Exec(q, status, id)
    elapsed := time.Since(start)

    if err != nil {
        logger.Error("Failed to update status", slog.Any("error", err), slog.Duration("elapsed", elapsed))
        return fmt.Errorf("update status: %w", err)
    }

    rows, _ := res.RowsAffected()
    logger.Info("Room status updated",
        slog.Int64("rows_affected", rows),
        slog.Duration("elapsed", elapsed))

    return nil
}

func (r *roomRepository) MarkEnded(id string, endedAt time.Time) error {
    logger := r.logger.With(
        slog.String("repo", "room"),
        slog.String("op", "mark_ended"),
        slog.String("room_id", id))

    q := `UPDATE rooms SET status = 'ended', ended_at = $1 WHERE id = $2`

    start := time.Now()
    res, err := r.db.Exec(q, endedAt, id)
    elapsed := time.Since(start)

    if err != nil {
        logger.Error("Failed to mark room as ended", slog.Any("error", err), slog.Duration("elapsed", elapsed))
        return fmt.Errorf("mark ended: %w", err)
    }

    rows, _ := res.RowsAffected()
    logger.Info("Room marked as ended",
        slog.Int64("rows_affected", rows),
        slog.Duration("elapsed", elapsed))

    return nil
}