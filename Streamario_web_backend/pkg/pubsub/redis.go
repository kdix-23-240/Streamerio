package pubsub

import (
	"context"
	"fmt"
	"log/slog"
	"time"

	"github.com/redis/go-redis/v9"
)

// redisPubSub: Redis Pub/Sub を利用した本番向け実装
type redisPubSub struct {
	rdb    *redis.Client
	logger *slog.Logger
}

// NewRedisPubSub: Redis Pub/Sub 実装を生成
func NewRedisPubSub(rdb *redis.Client, logger *slog.Logger) PubSub {
	if logger == nil {
		logger = slog.Default()
	}
	return &redisPubSub{
		rdb:    rdb,
		logger: logger,
	}
}

// Publish: Redis PUBLISH コマンドでメッセージを発行
func (r *redisPubSub) Publish(ctx context.Context, channel string, message []byte) error {
	logger := r.logger.With(
		slog.String("op", "publish"),
		slog.String("channel", channel),
		slog.Int("message_size", len(message)),
	)

	start := time.Now()
	receivers, err := r.rdb.Publish(ctx, channel, message).Result()
	if err != nil {
		logger.Error("redis.publish failed", slog.Any("error", err))
		return fmt.Errorf("redis publish failed: %w", err)
	}

	logger.Debug("redis.publish",
		slog.Int64("receivers", receivers),
		slog.Duration("elapsed", time.Since(start)),
	)
	return nil
}

// Subscribe: Redis SUBSCRIBE でチャネルを購読し、メッセージを受信
// context がキャンセルされるまでブロックし続ける
func (r *redisPubSub) Subscribe(ctx context.Context, channel string, handler MessageHandler) error {
	logger := r.logger.With(
		slog.String("op", "subscribe"),
		slog.String("channel", channel),
	)

	logger.Info("starting subscription")

	// Redis の購読を開始
	pubsub := r.rdb.Subscribe(ctx, channel)
	defer pubsub.Close()

	// 購読開始の確認
	if _, err := pubsub.Receive(ctx); err != nil {
		logger.Error("subscription failed", slog.Any("error", err))
		return fmt.Errorf("redis subscribe failed: %w", err)
	}

	logger.Info("subscription established")

	// メッセージ受信チャネル
	ch := pubsub.Channel()

	// メッセージループ
	for {
		select {
		case <-ctx.Done():
			logger.Info("subscription cancelled", slog.Any("reason", ctx.Err()))
			return ctx.Err()

		case msg, ok := <-ch:
			if !ok {
				logger.Warn("subscription channel closed")
				return fmt.Errorf("subscription channel closed")
			}

			// メッセージ処理
			start := time.Now()
			if err := handler(msg.Channel, []byte(msg.Payload)); err != nil {
				logger.Error("message handler error",
					slog.String("channel", msg.Channel),
					slog.Int("payload_size", len(msg.Payload)),
					slog.Any("error", err),
					slog.Duration("elapsed", time.Since(start)),
				)
				// エラーが発生しても購読は継続する
				continue
			}

			logger.Debug("message handled",
				slog.String("channel", msg.Channel),
				slog.Int("payload_size", len(msg.Payload)),
				slog.Duration("elapsed", time.Since(start)),
			)
		}
	}
}

// Close: リソースをクリーンアップ (Redis Client は外部管理のため何もしない)
func (r *redisPubSub) Close() error {
	return nil
}
