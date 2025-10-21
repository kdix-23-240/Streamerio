package repository

import (
    "database/sql"
    "fmt"
    "log/slog"
    "time"

    "streamerrio-backend/internal/model"

    "github.com/jmoiron/sqlx"
)

type ViewerRepository interface {
    Create(viewer *model.Viewer) error
    Exists(id string) (bool, error)
    Get(id string) (*model.Viewer, error)
}

type viewerRepository struct {
    db     *sqlx.DB
    logger *slog.Logger
}

func NewViewerRepository(db *sqlx.DB, logger *slog.Logger) ViewerRepository {
    if logger == nil {
        logger = slog.Default()
    }
    return &viewerRepository{db: db, logger: logger}
}

func (r *viewerRepository) Create(viewer *model.Viewer) error {
    if viewer.CreatedAt.IsZero() {
        viewer.CreatedAt = time.Now()
    }

    logger := r.logger.With(
        slog.String("repo", "viewer"),
        slog.String("op", "create"),
        slog.String("viewer_id", viewer.ID))

    q := `INSERT INTO viewers (id, name, created_at, updated_at) 
          VALUES ($1, $2, $3, $4)
          ON CONFLICT (id) DO UPDATE 
          SET name = EXCLUDED.name, updated_at = EXCLUDED.updated_at`

    start := time.Now()
    res, err := r.db.Exec(q, viewer.ID, viewer.Name, viewer.CreatedAt, viewer.UpdatedAt)
    elapsed := time.Since(start)

    if err != nil {
        logger.Error("Failed to create viewer", slog.Any("error", err), slog.Duration("elapsed", elapsed))
        return fmt.Errorf("create viewer: %w", err)
    }

    rows, _ := res.RowsAffected()
    logger.Debug("Viewer created/updated",
        slog.Int64("rows_affected", rows),
        slog.Duration("elapsed", elapsed))

    return nil
}

func (r *viewerRepository) Exists(id string) (bool, error) {
    logger := r.logger.With(
        slog.String("repo", "viewer"),
        slog.String("op", "exists"),
        slog.String("viewer_id", id))

    var exists bool
    q := `SELECT EXISTS(SELECT 1 FROM viewers WHERE id = $1)`

    start := time.Now()
    err := r.db.Get(&exists, q, id)
    elapsed := time.Since(start)

    if err != nil {
        logger.Error("Failed to check viewer existence", slog.Any("error", err), slog.Duration("elapsed", elapsed))
        return false, fmt.Errorf("check exists: %w", err)
    }

    logger.Debug("Viewer existence checked",
        slog.Bool("exists", exists),
        slog.Duration("elapsed", elapsed))

    return exists, nil
}

func (r *viewerRepository) Get(id string) (*model.Viewer, error) {
    logger := r.logger.With(
        slog.String("repo", "viewer"),
        slog.String("op", "get"),
        slog.String("viewer_id", id))

    var viewer model.Viewer
    q := `SELECT id, name, created_at, updated_at FROM viewers WHERE id = $1`

    start := time.Now()
    err := r.db.Get(&viewer, q, id)
    elapsed := time.Since(start)

    if err != nil {
        if err == sql.ErrNoRows {
            logger.Debug("Viewer not found", slog.Duration("elapsed", elapsed))
            return nil, nil
        }
        logger.Error("Failed to get viewer", slog.Any("error", err), slog.Duration("elapsed", elapsed))
        return nil, fmt.Errorf("get viewer: %w", err)
    }

    logger.Debug("Viewer retrieved", slog.Duration("elapsed", elapsed))
    return &viewer, nil
}