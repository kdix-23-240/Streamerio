package service

import (
	"fmt"
	"strings"
	"time"

	"streamerrio-backend/internal/model"
	"streamerrio-backend/internal/repository"

	"github.com/oklog/ulid/v2"
)

type ViewerService struct {
	repo repository.ViewerRepository
}

func NewViewerService(repo repository.ViewerRepository) *ViewerService {
	return &ViewerService{repo: repo}
}

// EnsureViewerID: 既存IDを確認し、存在しなければ新規発行して返す
func (s *ViewerService) EnsureViewerID(id string) (string, error) {
	if id != "" {
		exists, err := s.repo.Exists(id)
		if err == nil && exists {
			return id, nil
		}
	}
	newID := ulid.Make().String()
	now := time.Now()
	if err := s.repo.Create(&model.Viewer{ID: newID, CreatedAt: now, UpdatedAt: &now}); err != nil {
		return "", err
	}
	return newID, nil
}

func (s *ViewerService) SetViewerName(id, name string) (*model.Viewer, error) {
	if id == "" {
		return nil, fmt.Errorf("viewer_id required")
	}
	trimmed := strings.TrimSpace(name)
	var normalized *string
	if trimmed != "" {
		runes := []rune(trimmed)
		if len(runes) > 24 {
			runes = runes[:24]
			trimmed = string(runes)
		}
		normalized = &trimmed
	}
	now := time.Now()
	viewer := &model.Viewer{ID: id, Name: normalized, CreatedAt: now, UpdatedAt: &now}
	if err := s.repo.Create(viewer); err != nil {
		return nil, err
	}
	return s.repo.Get(id)
}

func (s *ViewerService) GetViewer(id string) (*model.Viewer, error) {
	return s.repo.Get(id)
}
