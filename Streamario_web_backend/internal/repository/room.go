package repository

import (
	"database/sql"
	"log/slog"
	"time"

	"streamerrio-backend/internal/model"

	"github.com/jmoiron/sqlx"
)

// RoomRepository: ルーム永続化アクセス用インタフェース
// 主要メソッドでクエリの所要時間と結果をログ出力する。
type RoomRepository interface {
	Create(room *model.Room) error                // 新規作成
	Get(id string) (*model.Room, error)           // ID取得 (存在しなければ nil)
	Delete(id string) error                       // ID削除
	Update(id string, room *model.Room) error     // ID更新
	MarkEnded(id string, endedAt time.Time) error // 終了状態に遷移
}

type roomRepository struct {
	db     *sqlx.DB
	logger *slog.Logger

	// 準備済みステートメント
	createStmt    *sqlx.Stmt
	getStmt       *sqlx.Stmt
	updateStmt    *sqlx.Stmt
	deleteStmt    *sqlx.Stmt
	markEndedStmt *sqlx.Stmt
}

// NewRoomRepository: 実装生成
func NewRoomRepository(db *sqlx.DB, logger *slog.Logger) RoomRepository {
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

	return &roomRepository{
		db:            db,
		logger:        logger,
		createStmt:    mustPrepare(queryCreateRoom),
		getStmt:       mustPrepare(queryGetRoom),
		updateStmt:    mustPrepare(queryUpdateRoom),
		deleteStmt:    mustPrepare(queryDeleteRoom),
		markEndedStmt: mustPrepare(queryMarkEndedRoom),
	}
}

// Create: rooms テーブルに挿入 (CreatedAt 未設定時は現在時刻)
func (r *roomRepository) Create(room *model.Room) error {
	if room.CreatedAt.IsZero() {
		room.CreatedAt = time.Now()
	}
	logger := r.logger.With(
		slog.String("repo", "room"),
		slog.String("op", "create"),
		slog.String("room_id", room.ID),
	)
	start := time.Now()
	res, err := r.createStmt.Exec(room.ID, room.StreamerID, room.CreatedAt, room.ExpiresAt, room.Status, room.Settings, room.EndedAt)
	if err != nil {
		logger.Error("db.exec (prepared) failed", slog.Any("error", err))
		return err
	}
	rows, _ := res.RowsAffected()
	logger.Debug("db.exec", slog.Int64("rows_affected", rows), slog.Duration("elapsed", time.Since(start)))
	return nil
}

// Get: 指定IDのルームを取得 (存在しない場合 nil を返す)
func (r *roomRepository) Get(id string) (*model.Room, error) {
	var rm model.Room
	q := `SELECT id, streamer_id, created_at, expires_at, status, settings, ended_at FROM rooms WHERE id=$1`
	logger := r.logger.With(
		slog.String("repo", "room"),
		slog.String("op", "get"),
		slog.String("room_id", id),
	)
	start := time.Now()
	if err := r.db.Get(&rm, q, id); err != nil {
		if err == sql.ErrNoRows {
			logger.Debug("db.query", slog.Bool("found", false), slog.Duration("elapsed", time.Since(start)))
			return nil, nil
		}
		logger.Error("db.query failed", slog.Any("error", err))
		return nil, err
	}
	logger.Debug("db.query", slog.Bool("found", true), slog.Duration("elapsed", time.Since(start)))
	return &rm, nil
}

// Update: 指定IDのルームを更新
func (r *roomRepository) Update(id string, room *model.Room) error {
	q := `UPDATE rooms SET streamer_id=$1, created_at=$2, expires_at=$3, status=$4, settings=$5, ended_at=$6 WHERE id=$7`
	logger := r.logger.With(
		slog.String("repo", "room"),
		slog.String("op", "update"),
		slog.String("room_id", id),
	)
	start := time.Now()
	res, err := r.db.Exec(q, room.StreamerID, room.CreatedAt, room.ExpiresAt, room.Status, room.Settings, room.EndedAt, id)
	if err != nil {
		logger.Error("db.exec failed", slog.Any("error", err))
		return err
	}
	rows, _ := res.RowsAffected()
	logger.Debug("db.exec", slog.Int64("rows_affected", rows), slog.Duration("elapsed", time.Since(start)))
	return nil
}

// Delete: 指定IDのルームを削除
func (r *roomRepository) Delete(id string) error {
	q := `DELETE FROM rooms WHERE id=$1`
	logger := r.logger.With(
		slog.String("repo", "room"),
		slog.String("op", "delete"),
		slog.String("room_id", id),
	)
	start := time.Now()
	res, err := r.db.Exec(q, id)
	if err != nil {
		logger.Error("db.exec failed", slog.Any("error", err))
		return err
	}
	rows, _ := res.RowsAffected()
	logger.Debug("db.exec", slog.Int64("rows_affected", rows), slog.Duration("elapsed", time.Since(start)))
	return nil
}

func (r *roomRepository) MarkEnded(id string, endedAt time.Time) error {
	q := `UPDATE rooms SET status=$1, ended_at=$2 WHERE id=$3`
	logger := r.logger.With(
		slog.String("repo", "room"),
		slog.String("op", "mark_ended"),
		slog.String("room_id", id),
	)
	start := time.Now()
	res, err := r.db.Exec(q, "ended", endedAt, id)
	if err != nil {
		logger.Error("db.exec failed", slog.Any("error", err))
		return err
	}
	rows, _ := res.RowsAffected()
	logger.Debug("db.exec", slog.Int64("rows_affected", rows), slog.Duration("elapsed", time.Since(start)))
	return nil
}
