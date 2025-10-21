package counter

import (
    "context"
    "fmt"
    "log/slog"
    "time"

    "github.com/redis/go-redis/v9"
)

type redisCounter struct {
    rdb    *redis.Client
    window time.Duration
    logger *slog.Logger
}

func NewRedisCounter(rdb *redis.Client, logger *slog.Logger) Counter {
    if logger == nil {
        logger = slog.Default()
    }
    return &redisCounter{
        rdb:    rdb,
        window: 5 * time.Minute,
        logger: logger,
    }
}

func (rc *redisCounter) keyCount(roomID, eventType string) string {
    return fmt.Sprintf("room:%s:cnt:%s", roomID, eventType)
}

func (rc *redisCounter) keyViewers(roomID string) string {
    return fmt.Sprintf("room:%s:viewers", roomID)
}

func (rc *redisCounter) Increment(roomID, eventType string) (int64, error) {
    key := rc.keyCount(roomID, eventType)
    logger := rc.logger.With(
        slog.String("counter", "redis"),
        slog.String("op", "increment"),
        slog.String("room_id", roomID),
        slog.String("event_type", eventType))

    ctx := context.Background()
    start := time.Now()
    val, err := rc.rdb.Incr(ctx, key).Result()
    elapsed := time.Since(start)

    if err != nil {
        logger.Error("Failed to increment", slog.Any("error", err), slog.Duration("elapsed", elapsed))
        return 0, fmt.Errorf("redis incr: %w", err)
    }

    logger.Debug("Counter incremented",
        slog.Int64("value", val),
        slog.Duration("elapsed", elapsed))

    return val, nil
}

func (rc *redisCounter) Get(roomID, eventType string) (int64, error) {
    key := rc.keyCount(roomID, eventType)
    logger := rc.logger.With(
        slog.String("counter", "redis"),
        slog.String("op", "get"),
        slog.String("room_id", roomID),
        slog.String("event_type", eventType))

    ctx := context.Background()
    start := time.Now()
    val, err := rc.rdb.Get(ctx, key).Int64()
    elapsed := time.Since(start)

    if err == redis.Nil {
        logger.Debug("Counter not found, returning 0", slog.Duration("elapsed", elapsed))
        return 0, nil
    }

    if err != nil {
        logger.Error("Failed to get counter", slog.Any("error", err), slog.Duration("elapsed", elapsed))
        return 0, fmt.Errorf("redis get: %w", err)
    }

    logger.Debug("Counter retrieved",
        slog.Int64("value", val),
        slog.Duration("elapsed", elapsed))

    return val, nil
}

func (rc *redisCounter) Reset(roomID, eventType string) error {
    key := rc.keyCount(roomID, eventType)
    logger := rc.logger.With(
        slog.String("counter", "redis"),
        slog.String("op", "reset"),
        slog.String("room_id", roomID),
        slog.String("event_type", eventType))

    ctx := context.Background()
    start := time.Now()
    err := rc.rdb.Del(ctx, key).Err()
    elapsed := time.Since(start)

    if err != nil {
        logger.Error("Failed to reset counter", slog.Any("error", err), slog.Duration("elapsed", elapsed))
        return fmt.Errorf("redis del: %w", err)
    }

    logger.Debug("Counter reset", slog.Duration("elapsed", elapsed))
    return nil
}

func (rc *redisCounter) UpdateViewerActivity(roomID, viewerID string) error {
    key := rc.keyViewers(roomID)
    logger := rc.logger.With(
        slog.String("counter", "redis"),
        slog.String("op", "update_viewer"),
        slog.String("room_id", roomID),
        slog.String("viewer_id", viewerID))

    now := float64(time.Now().Unix())
    ctx := context.Background()
    start := time.Now()

    // ZADDで視聴者のタイムスタンプを更新
    if err := rc.rdb.ZAdd(ctx, key, redis.Z{Score: now, Member: viewerID}).Err(); err != nil {
        elapsed := time.Since(start)
        logger.Error("Failed to update viewer activity", slog.Any("error", err), slog.Duration("elapsed", elapsed))
        return fmt.Errorf("redis zadd: %w", err)
    }

    // 古い視聴者を削除
    cutoff := float64(time.Now().Add(-rc.window).Unix())
    if err := rc.rdb.ZRemRangeByScore(ctx, key, "-inf", fmt.Sprintf("%f", cutoff-1)).Err(); err != nil {
        logger.Warn("Failed to clean old viewers", slog.Any("error", err))
    }

    elapsed := time.Since(start)
    logger.Debug("Viewer activity updated", slog.Duration("elapsed", elapsed))

    return nil
}

func (rc *redisCounter) GetActiveViewerCount(roomID string) (int64, error) {
    key := rc.keyViewers(roomID)
    logger := rc.logger.With(
        slog.String("counter", "redis"),
        slog.String("op", "get_active_viewers"),
        slog.String("room_id", roomID))

    cutoff := float64(time.Now().Add(-rc.window).Unix())
    ctx := context.Background()
    start := time.Now()

    count, err := rc.rdb.ZCount(ctx, key, fmt.Sprintf("%f", cutoff), "+inf").Result()
    elapsed := time.Since(start)

    if err != nil {
        logger.Error("Failed to count active viewers", slog.Any("error", err), slog.Duration("elapsed", elapsed))
        return 0, fmt.Errorf("redis zcount: %w", err)
    }

    logger.Debug("Active viewers counted",
        slog.Int64("count", count),
        slog.Duration("elapsed", elapsed))

    return count, nil
}