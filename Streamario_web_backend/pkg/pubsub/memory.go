package pubsub

import (
	"context"
	"fmt"
	"log/slog"
	"sync"
)

// memoryPubSub: テスト/開発用のインメモリ実装
// 同一プロセス内でのみ動作し、複数サーバー間では機能しない
type memoryPubSub struct {
	mu          sync.RWMutex
	subscribers map[string][]chan []byte // channel -> subscriber channels
	logger      *slog.Logger
	closed      bool
}

// NewMemoryPubSub: インメモリ Pub/Sub 実装を生成
func NewMemoryPubSub(logger *slog.Logger) PubSub {
	if logger == nil {
		logger = slog.Default()
	}
	return &memoryPubSub{
		subscribers: make(map[string][]chan []byte),
		logger:      logger,
	}
}

// Publish: メモリ内の購読者全員にメッセージを配信
func (m *memoryPubSub) Publish(ctx context.Context, channel string, message []byte) error {
	m.mu.RLock()
	defer m.mu.RUnlock()

	if m.closed {
		return fmt.Errorf("pubsub is closed")
	}

	logger := m.logger.With(
		slog.String("op", "publish"),
		slog.String("channel", channel),
		slog.Int("message_size", len(message)),
	)

	subs, exists := m.subscribers[channel]
	if !exists || len(subs) == 0 {
		logger.Debug("no subscribers", slog.Int("subscriber_count", 0))
		return nil
	}

	// メッセージをコピーして配信（元のスライスを変更しても影響がないように）
	msgCopy := make([]byte, len(message))
	copy(msgCopy, message)

	delivered := 0
	for _, sub := range subs {
		select {
		case sub <- msgCopy:
			delivered++
		case <-ctx.Done():
			return ctx.Err()
		default:
			// バッファがいっぱいの場合はスキップ
			logger.Warn("subscriber channel full, message dropped")
		}
	}

	logger.Debug("message published",
		slog.Int("subscriber_count", len(subs)),
		slog.Int("delivered", delivered),
	)
	return nil
}

// Subscribe: チャネルを購読し、メッセージを受信
func (m *memoryPubSub) Subscribe(ctx context.Context, channel string, handler MessageHandler) error {
	logger := m.logger.With(
		slog.String("op", "subscribe"),
		slog.String("channel", channel),
	)

	// 購読用チャネルを作成（バッファサイズ100）
	msgChan := make(chan []byte, 100)

	// 購読者リストに追加
	m.mu.Lock()
	if m.closed {
		m.mu.Unlock()
		return fmt.Errorf("pubsub is closed")
	}
	m.subscribers[channel] = append(m.subscribers[channel], msgChan)
	subIndex := len(m.subscribers[channel]) - 1
	m.mu.Unlock()

	logger.Info("subscription established")

	// context キャンセル時のクリーンアップを準備
	defer func() {
		m.mu.Lock()
		defer m.mu.Unlock()

		// 購読者リストから削除
		if subs, exists := m.subscribers[channel]; exists && subIndex < len(subs) {
			// チャネルをクローズ
			close(msgChan)

			// スライスから削除（順序は保持しない高速削除）
			subs[subIndex] = subs[len(subs)-1]
			m.subscribers[channel] = subs[:len(subs)-1]

			// 購読者がいなくなったらチャネルエントリ自体を削除
			if len(m.subscribers[channel]) == 0 {
				delete(m.subscribers, channel)
			}
		}
		logger.Info("subscription closed")
	}()

	// メッセージループ
	for {
		select {
		case <-ctx.Done():
			return ctx.Err()

		case msg, ok := <-msgChan:
			if !ok {
				return fmt.Errorf("subscription channel closed")
			}

			// ハンドラを呼び出し
			if err := handler(channel, msg); err != nil {
				logger.Error("message handler error",
					slog.Int("payload_size", len(msg)),
					slog.Any("error", err),
				)
				// エラーが発生しても購読は継続
			}
		}
	}
}

// Close: すべての購読を終了し、リソースをクリーンアップ
func (m *memoryPubSub) Close() error {
	m.mu.Lock()
	defer m.mu.Unlock()

	if m.closed {
		return nil
	}

	m.closed = true

	// すべての購読チャネルをクローズ
	for channel, subs := range m.subscribers {
		for _, sub := range subs {
			close(sub)
		}
		delete(m.subscribers, channel)
	}

	m.logger.Info("pubsub closed")
	return nil
}
