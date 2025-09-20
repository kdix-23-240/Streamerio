package repository

import (
	"database/sql"
	"streamerrio-backend/internal/model"
	"time"

	"github.com/jmoiron/sqlx"
)

// RoomRepository: ルーム永続化アクセス用インタフェース
type RoomRepository interface {
	Create(room *model.Room) error                // 新規作成
	Get(id string) (*model.Room, error)           // ID取得 (存在しなければ nil)
	Delete(id string) error                       // ID削除
	Update(id string, room *model.Room) error     // ID更新
	MarkEnded(id string, endedAt time.Time) error // 終了状態に遷移
}

type roomRepository struct{ db *sqlx.DB }

// NewRoomRepository: 実装生成
func NewRoomRepository(db *sqlx.DB) RoomRepository { return &roomRepository{db: db} }

// Create: rooms テーブルに挿入 (CreatedAt 未設定時は現在時刻)
func (r *roomRepository) Create(room *model.Room) error {
	if room.CreatedAt.IsZero() {
		room.CreatedAt = time.Now()
	}
	q := `INSERT INTO rooms (id, streamer_id, created_at, expires_at, status, settings, ended_at)
		  VALUES ($1,$2,$3,$4,$5,$6,$7)`
	_, err := r.db.Exec(q, room.ID, room.StreamerID, room.CreatedAt, room.ExpiresAt, room.Status, room.Settings, room.EndedAt)
	return err
}

// Get: 指定IDのルームを取得 (存在しない場合 nil を返す)
func (r *roomRepository) Get(id string) (*model.Room, error) {
	var rm model.Room
	q := `SELECT id, streamer_id, created_at, expires_at, status, settings, ended_at FROM rooms WHERE id=$1`
	if err := r.db.Get(&rm, q, id); err != nil {
		if err == sql.ErrNoRows {
			return nil, nil
		}
		return nil, err
	}
	return &rm, nil
}

// Update: 指定IDのルームを更新
func (r *roomRepository) Update(id string, room *model.Room) error {
	q := `UPDATE rooms SET streamer_id=$1, created_at=$2, expires_at=$3, status=$4, settings=$5, ended_at=$6 WHERE id=$7`
	_, err := r.db.Exec(q, room.StreamerID, room.CreatedAt, room.ExpiresAt, room.Status, room.Settings, room.EndedAt, id)
	return err
}

// Delete: 指定IDのルームを削除
func (r *roomRepository) Delete(id string) error {
	q := `DELETE FROM rooms WHERE id=$1`
	_, err := r.db.Exec(q, id)
	return err
}

func (r *roomRepository) MarkEnded(id string, endedAt time.Time) error {
	q := `UPDATE rooms SET status=$1, ended_at=$2 WHERE id=$3`
	_, err := r.db.Exec(q, "ended", endedAt, id)
	return err
}
