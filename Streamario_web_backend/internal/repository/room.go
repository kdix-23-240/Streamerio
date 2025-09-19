package repository

import (
	"database/sql"
	"streamerrio-backend/internal/model"
	"time"

	"github.com/jmoiron/sqlx"
)

// RoomRepository: ルーム永続化アクセス用インタフェース
type RoomRepository interface {
	Create(room *model.Room) error     // 新規作成
	Get(id string) (*model.Room, error) // ID取得 (存在しなければ nil)
}

type roomRepository struct{ db *sqlx.DB }

// NewRoomRepository: 実装生成
func NewRoomRepository(db *sqlx.DB) RoomRepository { return &roomRepository{db: db} }

// Create: rooms テーブルに挿入 (CreatedAt 未設定時は現在時刻)
func (r *roomRepository) Create(room *model.Room) error {
	if room.CreatedAt.IsZero() {
		room.CreatedAt = time.Now()
	}
	q := `INSERT INTO rooms (id, streamer_id, created_at, expires_at, status, settings)
		  VALUES ($1,$2,$3,$4,$5,$6)`
	_, err := r.db.Exec(q, room.ID, room.StreamerID, room.CreatedAt, room.ExpiresAt, room.Status, room.Settings)
	return err
}

// Get: 指定IDのルームを取得 (存在しない場合 nil を返す)
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
