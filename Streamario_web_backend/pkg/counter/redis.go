package counter

import (
	"context"
	"fmt"
	"time"

	"github.com/redis/go-redis/v9"
)

type redisCounter struct {
	rdb    *redis.Client
	window time.Duration
}

func NewRedisCounter(rdb *redis.Client) Counter {
	return &redisCounter{rdb: rdb, window: 5 * time.Minute}
}

func (rc *redisCounter) keyCount(roomID, eventType string) string {
	return fmt.Sprintf("room:%s:cnt:%s", roomID, eventType)
}
func (rc *redisCounter) keyViewers(roomID string) string {
	return fmt.Sprintf("room:%s:viewers", roomID)
}

func (rc *redisCounter) Increment(roomID, eventType string) (int64, error) {
	return rc.rdb.Incr(context.Background(), rc.keyCount(roomID, eventType)).Result()
}

func (rc *redisCounter) Get(roomID, eventType string) (int64, error) {
	v, err := rc.rdb.Get(context.Background(), rc.keyCount(roomID, eventType)).Int64()
	if err == redis.Nil {
		return 0, nil
	}
	return v, err
}

func (rc *redisCounter) Reset(roomID, eventType string) error {
	return rc.rdb.Del(context.Background(), rc.keyCount(roomID, eventType)).Err()
}

func (rc *redisCounter) UpdateViewerActivity(roomID, viewerID string) error {
	now := float64(time.Now().Unix())
	key := rc.keyViewers(roomID)
	ctx := context.Background()
	if err := rc.rdb.ZAdd(ctx, key, redis.Z{Score: now, Member: viewerID}).Err(); err != nil {
		return err
	}
	cutoff := float64(time.Now().Add(-rc.window).Unix())
	return rc.rdb.ZRemRangeByScore(ctx, key, "-inf", fmt.Sprintf("%f", cutoff-1)).Err()
}

func (rc *redisCounter) GetActiveViewerCount(roomID string) (int64, error) {
	key := rc.keyViewers(roomID)
	cutoff := float64(time.Now().Add(-rc.window).Unix())
	ctx := context.Background()
	return rc.rdb.ZCount(ctx, key, fmt.Sprintf("%f", cutoff), "+inf").Result()
}
