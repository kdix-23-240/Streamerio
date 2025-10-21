package config

import (
    "fmt"
    "log/slog"
    "math"
    "sort"
    "sync"
)

// GameConfig: ゲームロジック設定の集中管理
// 閾値計算とビューア倍率をスレッドセーフに管理
type GameConfig struct {
    mu                 sync.RWMutex
    eventConfigs       map[string]*EventConfig
    viewerMultipliers  []ViewerMultiplier
    logger             *slog.Logger
}

// EventConfig: イベント種別ごとの閾値設定
type EventConfig struct {
    EventType     string  `json:"event_type"`
    BaseThreshold int     `json:"base_threshold"`
    MinThreshold  int     `json:"min_threshold"`
    MaxThreshold  int     `json:"max_threshold"`
}

// ViewerMultiplier: 視聴者数に応じた倍率
type ViewerMultiplier struct {
    MaxViewers int     `json:"max_viewers"`
    Multiplier float64 `json:"multiplier"`
}

// NewGameConfig: デフォルト設定で初期化
func NewGameConfig(logger *slog.Logger) *GameConfig {
    if logger == nil {
        logger = slog.Default()
    }

    gc := &GameConfig{
        eventConfigs:      make(map[string]*EventConfig),
        viewerMultipliers: getDefaultViewerMultipliers(),
        logger:            logger,
    }

    // デフォルトイベント設定
    defaults := []EventConfig{
        {"skill1", 5, 3, 50},
        {"skill2", 6, 4, 60},
        {"skill3", 12, 8, 100},
        {"enemy1", 6, 4, 45},
        {"enemy2", 7, 5, 55},
        {"enemy3", 10, 6, 80},
    }

    for _, cfg := range defaults {
        gc.eventConfigs[cfg.EventType] = &EventConfig{
            EventType:     cfg.EventType,
            BaseThreshold: cfg.BaseThreshold,
            MinThreshold:  cfg.MinThreshold,
            MaxThreshold:  cfg.MaxThreshold,
        }
    }

    logger.Info("GameConfig initialized with defaults",
        slog.Int("event_types", len(gc.eventConfigs)),
        slog.Int("multiplier_brackets", len(gc.viewerMultipliers)))

    return gc
}

// CalculateThreshold: 動的閾値計算（視聴者数を考慮）
func (gc *GameConfig) CalculateThreshold(eventType string, viewerCount int) (int, error) {
    gc.mu.RLock()
    defer gc.mu.RUnlock()

    cfg, ok := gc.eventConfigs[eventType]
    if !ok {
        return 0, fmt.Errorf("unknown event type: %s", eventType)
    }

    multiplier := gc.getMultiplierForViewers(viewerCount)
    raw := float64(cfg.BaseThreshold) * multiplier
    threshold := int(math.Ceil(raw))

    // 上下限クランプ
    if threshold < cfg.MinThreshold {
        threshold = cfg.MinThreshold
    }
    if threshold > cfg.MaxThreshold {
        threshold = cfg.MaxThreshold
    }

    gc.logger.Debug("Threshold calculated",
        slog.String("event_type", eventType),
        slog.Int("viewer_count", viewerCount),
        slog.Float64("multiplier", multiplier),
        slog.Int("raw", int(raw)),
        slog.Int("final", threshold),
        slog.Int("min", cfg.MinThreshold),
        slog.Int("max", cfg.MaxThreshold))

    return threshold, nil
}

// getMultiplierForViewers: 視聴者数に応じた倍率を返す（内部ヘルパー）
func (gc *GameConfig) getMultiplierForViewers(count int) float64 {
    // multipliers は MaxViewers 昇順にソート済み前提
    for _, m := range gc.viewerMultipliers {
        if count <= m.MaxViewers {
            return m.Multiplier
        }
    }
    // フォールバック（最大ブラケット）
    if len(gc.viewerMultipliers) > 0 {
        return gc.viewerMultipliers[len(gc.viewerMultipliers)-1].Multiplier
    }
    return 1.0
}

// UpdateEventConfig: イベント設定を動的更新
func (gc *GameConfig) UpdateEventConfig(eventType string, base, min, max int) error {
    if base < 1 || min < 1 || max < base {
        return fmt.Errorf("invalid thresholds: base=%d min=%d max=%d", base, min, max)
    }

    gc.mu.Lock()
    defer gc.mu.Unlock()

    cfg, ok := gc.eventConfigs[eventType]
    if !ok {
        return fmt.Errorf("unknown event type: %s", eventType)
    }

    oldBase := cfg.BaseThreshold
    cfg.BaseThreshold = base
    cfg.MinThreshold = min
    cfg.MaxThreshold = max

    gc.logger.Info("Event config updated",
        slog.String("event_type", eventType),
        slog.Int("old_base", oldBase),
        slog.Int("new_base", base),
        slog.Int("min", min),
        slog.Int("max", max))

    return nil
}

// SetViewerMultipliers: 倍率テーブルを置き換え
func (gc *GameConfig) SetViewerMultipliers(multipliers []ViewerMultiplier) error {
    if len(multipliers) == 0 {
        return fmt.Errorf("multipliers cannot be empty")
    }

    // 検証: MaxViewers 昇順、Multiplier > 0
    for i, m := range multipliers {
        if m.Multiplier <= 0 {
            return fmt.Errorf("multiplier[%d] invalid: %f", i, m.Multiplier)
        }
        if i > 0 && multipliers[i-1].MaxViewers >= m.MaxViewers {
            return fmt.Errorf("multipliers must be sorted by max_viewers")
        }
    }

    gc.mu.Lock()
    defer gc.mu.Unlock()

    gc.viewerMultipliers = multipliers

    gc.logger.Info("Viewer multipliers updated",
        slog.Int("count", len(multipliers)))

    return nil
}

// GetAllConfigs: すべての設定を取得（管理API用）
func (gc *GameConfig) GetAllConfigs() map[string]interface{} {
    gc.mu.RLock()
    defer gc.mu.RUnlock()

    configs := make(map[string]*EventConfig, len(gc.eventConfigs))
    for k, v := range gc.eventConfigs {
        configs[k] = &EventConfig{
            EventType:     v.EventType,
            BaseThreshold: v.BaseThreshold,
            MinThreshold:  v.MinThreshold,
            MaxThreshold:  v.MaxThreshold,
        }
    }

    return map[string]interface{}{
        "event_configs":       configs,
        "viewer_multipliers":  gc.viewerMultipliers,
    }
}

// ListEventTypes: 登録されているイベント種別一覧
func (gc *GameConfig) ListEventTypes() []string {
    gc.mu.RLock()
    defer gc.mu.RUnlock()

    types := make([]string, 0, len(gc.eventConfigs))
    for et := range gc.eventConfigs {
        types = append(types, et)
    }
    sort.Strings(types)
    return types
}

// デフォルト倍率テーブル
func getDefaultViewerMultipliers() []ViewerMultiplier {
    return []ViewerMultiplier{
        {MaxViewers: 5, Multiplier: 1.0},
        {MaxViewers: 10, Multiplier: 1.2},
        {MaxViewers: 20, Multiplier: 1.5},
        {MaxViewers: 50, Multiplier: 2.0},
        {MaxViewers: 999999, Multiplier: 3.0},
    }
}