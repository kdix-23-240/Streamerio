package service

import (
    "fmt"
    "log/slog"
    "strings"
    "time"

    "streamerrio-backend/internal/model"
    "streamerrio-backend/internal/repository"

    "github.com/oklog/ulid/v2"
)

type ViewerService struct {
    repo   repository.ViewerRepository
    logger *slog.Logger
}

func NewViewerService(repo repository.ViewerRepository, logger *slog.Logger) *ViewerService {
    if logger == nil {
        logger = slog.Default()
    }
    return &ViewerService{
        repo:   repo,
        logger: logger,
    }
}

func (s *ViewerService) EnsureViewerID(id string) (string, error) {
    logger := s.logger.With(
        slog.String("service", "viewer"),
        slog.String("op", "ensure_id"))

    if id != "" {
        logger.Debug("Checking existing viewer ID", slog.String("viewer_id", id))
        exists, err := s.repo.Exists(id)
        if err == nil && exists {
            logger.Debug("Viewer ID exists, reusing")
            return id, nil
        }
        if err != nil {
            logger.Warn("Failed to check viewer existence", slog.Any("error", err))
        }
    }

    newID := ulid.Make().String()
    logger.Info("Generating new viewer ID", slog.String("viewer_id", newID))

    now := time.Now()
    if err := s.repo.Create(&model.Viewer{
        ID:        newID,
        CreatedAt: now,
        UpdatedAt: &now,
    }); err != nil {
        logger.Error("Failed to create viewer", slog.Any("error", err))
        return "", fmt.Errorf("create viewer: %w", err)
    }

    logger.Info("New viewer created successfully")
    return newID, nil
}

func (s *ViewerService) SetViewerName(id, name string) (*model.Viewer, error) {
    logger := s.logger.With(
        slog.String("service", "viewer"),
        slog.String("op", "set_name"),
        slog.String("viewer_id", id))

    if id == "" {
        logger.Warn("Viewer ID required")
        return nil, fmt.Errorf("viewer_id required")
    }

    trimmed := strings.TrimSpace(name)
    var normalized *string
    if trimmed != "" {
        runes := []rune(trimmed)
        if len(runes) > 24 {
            logger.Debug("Trimming viewer name",
                slog.Int("original_length", len(runes)),
                slog.Int("trimmed_length", 24))
            runes = runes[:24]
            trimmed = string(runes)
        }
        normalized = &trimmed
    }

    logger.Info("Setting viewer name",
        slog.String("name", trimmed))

    now := time.Now()
    viewer := &model.Viewer{
        ID:        id,
        Name:      normalized,
        CreatedAt: now,
        UpdatedAt: &now,
    }

    if err := s.repo.Create(viewer); err != nil {
        logger.Error("Failed to set viewer name", slog.Any("error", err))
        return nil, fmt.Errorf("set viewer name: %w", err)
    }

    result, err := s.repo.Get(id)
    if err != nil {
        logger.Error("Failed to retrieve updated viewer", slog.Any("error", err))
        return nil, fmt.Errorf("get viewer: %w", err)
    }

    logger.Info("Viewer name set successfully")
    return result, nil
}

func (s *ViewerService) GetViewer(id string) (*model.Viewer, error) {
    logger := s.logger.With(
        slog.String("service", "viewer"),
        slog.String("op", "get"),
        slog.String("viewer_id", id))

    logger.Debug("Fetching viewer")

    viewer, err := s.repo.Get(id)
    if err != nil {
        logger.Error("Failed to get viewer", slog.Any("error", err))
        return nil, fmt.Errorf("get viewer: %w", err)
    }

    if viewer == nil {
        logger.Debug("Viewer not found")
        return nil, nil
    }

    logger.Debug("Viewer retrieved successfully")
    return viewer, nil
}