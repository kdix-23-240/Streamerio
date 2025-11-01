package repository

import (
	"database/sql"
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

	// 準備済みステートメント
	createStmt *sqlx.Stmt
	existsStmt *sqlx.Stmt
	getStmt    *sqlx.Stmt
}

func NewViewerRepository(db *sqlx.DB, logger *slog.Logger) ViewerRepository {
	if logger == nil {
		logger = slog.Default()
	}

	mustPrepare := func(query string) *sqlx.Stmt {
		stmt, err := db.Preparex(query)
		if err != nil {
			logger.Error("failed to prepare statement", slog.Any("error", err), slog.String("query", query))
			panic(err)
		}
		return stmt
	}

	return &viewerRepository{
		db:         db,
		logger:     logger,
		createStmt: mustPrepare(queryCreateViewer),
		existsStmt: mustPrepare(queryExistsViewer),
		getStmt:    mustPrepare(queryGetViewer),
	}
}

func (r *viewerRepository) Create(viewer *model.Viewer) error {
	if viewer.CreatedAt.IsZero() {
		viewer.CreatedAt = time.Now()
	}
	q := `INSERT INTO viewers (id, name, created_at, updated_at) VALUES ($1, $2, $3, $4)
        ON CONFLICT (id) DO UPDATE SET name = EXCLUDED.name, updated_at = EXCLUDED.updated_at`
	logger := r.logger.With(
		slog.String("repo", "viewer"),
		slog.String("op", "create"),
		slog.String("viewer_id", viewer.ID),
	)
	start := time.Now()
	res, err := r.db.Exec(q, viewer.ID, viewer.Name, viewer.CreatedAt, viewer.UpdatedAt)
	if err != nil {
		logger.Error("db.exec failed", slog.Any("error", err))
		return err
	}
	rows, _ := res.RowsAffected()
	logger.Debug("db.exec", slog.Int64("rows_affected", rows), slog.Duration("elapsed", time.Since(start)))
	return nil
}

func (r *viewerRepository) Exists(id string) (bool, error) {
	var exists bool
	q := `SELECT EXISTS(SELECT 1 FROM viewers WHERE id = $1)`
	logger := r.logger.With(
		slog.String("repo", "viewer"),
		slog.String("op", "exists"),
		slog.String("viewer_id", id),
	)
	start := time.Now()
	if err := r.db.Get(&exists, q, id); err != nil {
		logger.Error("db.query failed", slog.Any("error", err))
		return false, err
	}
	logger.Debug("db.query", slog.Bool("exists", exists), slog.Duration("elapsed", time.Since(start)))
	return exists, nil
}

func (r *viewerRepository) Get(id string) (*model.Viewer, error) {
	var viewer model.Viewer
	q := `SELECT id, name, created_at, updated_at FROM viewers WHERE id = $1`
	logger := r.logger.With(
		slog.String("repo", "viewer"),
		slog.String("op", "get"),
		slog.String("viewer_id", id),
	)
	start := time.Now()
	if err := r.db.Get(&viewer, q, id); err != nil {
		if err == sql.ErrNoRows {
			logger.Debug("db.query", slog.Bool("found", false), slog.Duration("elapsed", time.Since(start)))
			return nil, nil
		}
		logger.Error("db.query failed", slog.Any("error", err))
		return nil, err
	}
	logger.Debug("db.query", slog.Bool("found", true), slog.Duration("elapsed", time.Since(start)))
	return &viewer, nil
}
