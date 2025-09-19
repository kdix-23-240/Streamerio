package repository

import (
	"database/sql"
	"streamerrio-backend/internal/model"
	"time"

	"github.com/jmoiron/sqlx"
)

type RoomRepository interface {
	Create(room *model.Room) error
	Get(id string) (*model.Room, error)
}

type roomRepository struct{ db *sqlx.DB }

func NewRoomRepository(db *sqlx.DB) RoomRepository { return &roomRepository{db: db} }

func (r *roomRepository) Create(room *model.Room) error {
	if room.CreatedAt.IsZero() {
		room.CreatedAt = time.Now()
	}
	q := `INSERT INTO rooms (id, streamer_id, created_at, expires_at, status, settings)
		  VALUES ($1,$2,$3,$4,$5,$6)`
	_, err := r.db.Exec(q, room.ID, room.StreamerID, room.CreatedAt, room.ExpiresAt, room.Status, room.Settings)
	return err
}

func (r *roomRepository) Get(id string) (*model.Room, error) {
	var rm model.Room
	q := `SELECT id, streamer_id, created_at, expires_at, status, settings FROM rooms WHERE id=$1`
	if err := r.db.Get(&rm, q, id); err != nil {
		if err == sql.ErrNoRows {
			return nil, nil
		}
		return nil, err
	}
	return &rm, nil
}
