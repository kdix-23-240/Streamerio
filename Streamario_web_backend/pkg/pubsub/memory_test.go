package pubsub

import (
	"context"
	"log/slog"
	"sync"
	"testing"
	"time"
)

func TestMemoryPubSub_PublishSubscribe(t *testing.T) {
	logger := slog.New(slog.NewTextHandler(nil, &slog.HandlerOptions{Level: slog.LevelError}))
	ps := NewMemoryPubSub(logger)
	defer ps.Close()

	ctx, cancel := context.WithTimeout(context.Background(), 5*time.Second)
	defer cancel()

	channel := "test_channel"
	testMessage := []byte("hello world")

	// 受信メッセージを格納
	var receivedMu sync.Mutex
	var receivedMessages [][]byte

	handler := func(ch string, msg []byte) error {
		receivedMu.Lock()
		defer receivedMu.Unlock()
		receivedMessages = append(receivedMessages, msg)
		return nil
	}

	// 購読を開始（別goroutine）
	subCtx, subCancel := context.WithCancel(ctx)
	var wg sync.WaitGroup
	wg.Add(1)
	go func() {
		defer wg.Done()
		_ = ps.Subscribe(subCtx, channel, handler)
	}()

	// 購読が確立されるまで少し待つ
	time.Sleep(100 * time.Millisecond)

	// メッセージを発行
	if err := ps.Publish(ctx, channel, testMessage); err != nil {
		t.Fatalf("Publish failed: %v", err)
	}

	// メッセージが届くまで待つ
	time.Sleep(100 * time.Millisecond)

	// 購読をキャンセル
	subCancel()
	wg.Wait()

	// 検証
	receivedMu.Lock()
	defer receivedMu.Unlock()

	if len(receivedMessages) != 1 {
		t.Fatalf("expected 1 message, got %d", len(receivedMessages))
	}

	if string(receivedMessages[0]) != string(testMessage) {
		t.Errorf("expected message %q, got %q", testMessage, receivedMessages[0])
	}
}

func TestMemoryPubSub_MultipleSubscribers(t *testing.T) {
	logger := slog.New(slog.NewTextHandler(nil, &slog.HandlerOptions{Level: slog.LevelError}))
	ps := NewMemoryPubSub(logger)
	defer ps.Close()

	ctx, cancel := context.WithTimeout(context.Background(), 5*time.Second)
	defer cancel()

	channel := "test_channel"
	testMessage := []byte("broadcast")

	// 複数の購読者
	const subscriberCount = 3
	var counters [subscriberCount]int
	var countersMu sync.Mutex

	var wg sync.WaitGroup
	for i := 0; i < subscriberCount; i++ {
		subCtx, subCancel := context.WithCancel(ctx)
		defer subCancel()

		idx := i
		handler := func(ch string, msg []byte) error {
			countersMu.Lock()
			counters[idx]++
			countersMu.Unlock()
			return nil
		}

		wg.Add(1)
		go func() {
			defer wg.Done()
			_ = ps.Subscribe(subCtx, channel, handler)
		}()
	}

	// 購読が確立されるまで待つ
	time.Sleep(200 * time.Millisecond)

	// メッセージを発行
	if err := ps.Publish(ctx, channel, testMessage); err != nil {
		t.Fatalf("Publish failed: %v", err)
	}

	// メッセージが届くまで待つ
	time.Sleep(200 * time.Millisecond)

	// すべての購読をキャンセル
	cancel()
	wg.Wait()

	// 検証：すべての購読者がメッセージを受信
	countersMu.Lock()
	defer countersMu.Unlock()

	for i, count := range counters {
		if count != 1 {
			t.Errorf("subscriber %d: expected 1 message, got %d", i, count)
		}
	}
}

func TestMemoryPubSub_NoSubscribers(t *testing.T) {
	logger := slog.New(slog.NewTextHandler(nil, &slog.HandlerOptions{Level: slog.LevelError}))
	ps := NewMemoryPubSub(logger)
	defer ps.Close()

	ctx := context.Background()
	channel := "test_channel"
	testMessage := []byte("nobody listening")

	// 購読者なしでもエラーにならない
	if err := ps.Publish(ctx, channel, testMessage); err != nil {
		t.Fatalf("Publish with no subscribers should not fail: %v", err)
	}
}

func TestMemoryPubSub_Close(t *testing.T) {
	logger := slog.New(slog.NewTextHandler(nil, &slog.HandlerOptions{Level: slog.LevelError}))
	ps := NewMemoryPubSub(logger)

	if err := ps.Close(); err != nil {
		t.Fatalf("Close failed: %v", err)
	}

	// Closeした後のPublishはエラー
	ctx := context.Background()
	if err := ps.Publish(ctx, "test", []byte("msg")); err == nil {
		t.Error("expected error after close, got nil")
	}
}
