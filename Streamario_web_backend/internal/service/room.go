package service

import (
    "crypto/rand"
    "errors"
    "time"

    "github.com/oklog/ulid/v2"
    "streamerrio-backend/internal/config"
    "streamerrio-backend/internal/model"
    "streamerrio-backend/internal/repository"
)

// RoomService handles room lifecycle.
type RoomService struct {
	repo repository.RoomRepository
	cfg  *config.Config
}

func NewRoomService(repo repository.RoomRepository, cfg *config.Config) *RoomService {
	return &RoomService{repo: repo, cfg: cfg}
}

func (s *RoomService) CreateRoom(room *model.Room) error {
	if room.ID == "" {
		return errors.New("room id required")
	}
	if room.CreatedAt.IsZero() { room.CreatedAt = time.Now() }
	return s.repo.Create(room)
}

func (s *RoomService) GetRoom(id string) (*model.Room, error) {
	room, err := s.repo.Get(id)
	if err != nil { return nil, err }
	if room == nil { return nil, errors.New("room not found") }
	if room.ExpiresAt != nil && time.Now().After(*room.ExpiresAt) {
		return nil, errors.New("room expired")
	}
	return room, nil
}

// GenerateRoom creates a new room with ULID id.
func (s *RoomService) GenerateRoom(streamerID string) (*model.Room, error) {
	entropy := ulid.Monotonic(rand.Reader, 0)
	id := ulid.MustNew(ulid.Timestamp(time.Now()), entropy).String()
	room := &model.Room{
		ID:         id,
		StreamerID: streamerID,
		CreatedAt:  time.Now(),
		Status:     "active",
		Settings:   "{}",
	}
	// TTL機能は現行 Config に無いため省略（必要なら Config に追加予定）
	if err := s.repo.Create(room); err != nil { return nil, err }
	return room, nil
}

// EnsureRoom returns existing room or creates a minimal one with given id.
func (s *RoomService) EnsureRoom(id string) (*model.Room, error) {
	existing, err := s.repo.Get(id)
	if err != nil { return nil, err }
	if existing != nil { return existing, nil }
	rm := &model.Room{ID: id, StreamerID: "anonymous", CreatedAt: time.Now(), Status: "active", Settings: "{}"}
	if err := s.repo.Create(rm); err != nil { return nil, err }
	return rm, nil
}
