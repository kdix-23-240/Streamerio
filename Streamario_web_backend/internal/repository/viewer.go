package repository

import (
	"database/sql"
	"time"

	"streamerrio-backend/internal/model"

	"github.com/jmoiron/sqlx"
)

type ViewerRepository interface {
	Create(viewer *model.Viewer) error
	Exists(id string) (bool, error)
	Get(id string) (*model.Viewer, error)
}

type viewerRepository struct{ db *sqlx.DB }

func NewViewerRepository(db *sqlx.DB) ViewerRepository { return &viewerRepository{db: db} }

func (r *viewerRepository) Create(viewer *model.Viewer) error {
	if viewer.CreatedAt.IsZero() {
		viewer.CreatedAt = time.Now()
	}
	q := `INSERT INTO viewers (id, name, created_at, updated_at) VALUES ($1, $2, $3, $4)
		ON CONFLICT (id) DO UPDATE SET name = EXCLUDED.name, updated_at = EXCLUDED.updated_at`
	_, err := r.db.Exec(q, viewer.ID, viewer.Name, viewer.CreatedAt, viewer.UpdatedAt)
	return err
}

func (r *viewerRepository) Exists(id string) (bool, error) {
	var exists bool
	q := `SELECT EXISTS(SELECT 1 FROM viewers WHERE id = $1)`
	if err := r.db.Get(&exists, q, id); err != nil {
		return false, err
	}
	return exists, nil
}

func (r *viewerRepository) Get(id string) (*model.Viewer, error) {
	var viewer model.Viewer
	q := `SELECT id, name, created_at, updated_at FROM viewers WHERE id = $1`
	if err := r.db.Get(&viewer, q, id); err != nil {
		if err == sql.ErrNoRows {
			return nil, nil
		}
		return nil, err
	}
	return &viewer, nil
}
