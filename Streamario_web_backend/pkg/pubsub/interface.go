package pubsub

import "context"

// PubSub: メッセージング抽象化インタフェース
// REST API と WebSocket サーバー間のイベント配信に使用
type PubSub interface {
	// Publish: 指定チャネルにメッセージを発行
	Publish(ctx context.Context, channel string, message []byte) error

	// Subscribe: チャネルを購読し、メッセージ受信時にハンドラを呼ぶ
	// context がキャンセルされるまでブロックし続ける
	Subscribe(ctx context.Context, channel string, handler MessageHandler) error

	// Close: リソースをクリーンアップ
	Close() error
}

// MessageHandler: メッセージ受信時のコールバック関数
// エラーを返した場合はログに記録されるが、購読は継続する
type MessageHandler func(channel string, message []byte) error
