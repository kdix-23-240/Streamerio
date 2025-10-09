# PubSub パッケージ

## 概要

REST APIサーバーとWebSocketサーバー間のイベント配信を抽象化したPub/Sub実装。

## 設計意図

### 背景
- バックエンドをREST APIとWebSocketサーバーに分離する際、サーバー間でイベントを配信する仕組みが必要
- 既存の`pkg/counter`パッケージと同様に、インターフェースで抽象化し、Redis実装とメモリ実装を提供

### アーキテクチャ
```
[REST API Server]
    ↓ Publish("game_events", event)
[Redis Pub/Sub]
    ↓ Subscribe("game_events")
[WebSocket Server(s)] → Unity
```

## 実装方針

### 高凝集・低結合
- **インターフェース分離**: `PubSub`インターフェースで実装を抽象化
- **実装の交換可能性**: Redis/Memoryを環境に応じて切り替え可能
- **依存性注入**: 使用側はインターフェースに依存し、具体実装は外部から注入

### 既存コードとの統一
- `pkg/counter`パッケージと同じ構造（interface.go, redis.go, memory.go）
- ログ出力スタイル（slog使用、elapsed計測）
- エラーハンドリング（errors.Wrapでコンテキスト保持）

## 使用例

### 1. 初期化（REST APIサーバー側）

```go
import (
    "streamerrio-backend/pkg/pubsub"
    "github.com/redis/go-redis/v9"
)

// Redis Client初期化
rdb := redis.NewClient(&redis.Options{Addr: "localhost:6379"})

// PubSub初期化
ps := pubsub.NewRedisPubSub(rdb, logger)
```

### 2. メッセージ発行（REST APIサーバー）

```go
// イベントをJSON化
payload := map[string]interface{}{
    "type": "game_event",
    "room_id": roomID,
    "event_type": "skill1",
}
message, _ := json.Marshal(payload)

// チャネルに発行（定数を使用）
err := ps.Publish(ctx, pubsub.ChannelGameEvents, message)
```

### 3. メッセージ購読（WebSocketサーバー）

```go
// メッセージハンドラを定義
handler := func(channel string, message []byte) error {
    var payload map[string]interface{}
    if err := json.Unmarshal(message, &payload); err != nil {
        return err
    }
    
    roomID := payload["room_id"].(string)
    
    // WebSocketで該当Unityクライアントに配信
    return wsHandler.SendEventToUnity(roomID, payload)
}

// 購読開始（ブロッキング、定数を使用）
err := ps.Subscribe(ctx, pubsub.ChannelGameEvents, handler)
```

## チャネル設計

チャネル名は `channels.go` で定数定義されています。タイポ防止のため、必ず定数を使用してください。

### pubsub.ChannelGameEvents
REST APIで受信したイベントをWebSocketサーバーへ配信

**チャネル名**: `"game_events"`

**Payload例:**
```json
{
  "type": "game_event",
  "room_id": "01HXXX...",
  "event_type": "skill1",
  "trigger_count": 5,
  "viewer_count": 12
}
```

### pubsub.ChannelGameEnd
ゲーム終了通知（将来拡張用）

**チャネル名**: `"game_end_notifications"`

**Payload例:**
```json
{
  "type": "game_end_summary",
  "room_id": "01HXXX...",
  "top_by_button": {...},
  "top_overall": {...}
}
```

## 実装詳細

### Redis実装 (`redis.go`)
- **本番環境向け**: 複数サーバー間でメッセージ配信可能
- **Publish**: `PUBLISH`コマンドでメッセージ発行
- **Subscribe**: `SUBSCRIBE`でチャネル購読、contextキャンセルまでブロック
- **ログ**: 発行先数、処理時間を記録

### Memory実装 (`memory.go`)
- **開発/テスト向け**: 同一プロセス内のみで動作
- **軽量**: 外部依存なし、Redisが不要な環境で使用
- **制限**: サーバー分離時は機能しない

## エラーハンドリング

- **Publishエラー**: Redisへの接続エラー、ネットワークエラー
- **Subscribeエラー**: 購読開始失敗、チャネルクローズ
- **Handlerエラー**: ハンドラ内のエラーはログ記録のみ、購読は継続

## パフォーマンス考慮

- **非同期処理**: Publishは即座にreturn、配信はRedis/購読者が非同期処理
- **バッファリング**: Memory実装はチャネルバッファ（100）でバースト対応
- **リソース管理**: Subscribe終了時に自動クリーンアップ

## 今後の拡張

- **パターンマッチ購読**: `PSUBSCRIBE`による複数チャネル購読
- **メッセージ永続化**: Redis Streamsへの移行検討
- **リトライ機構**: 一時的なエラー時の自動再接続
- **メトリクス**: Publishedメッセージ数、処理レイテンシの計測

## 参考

- [Redis Pub/Sub](https://redis.io/docs/manual/pubsub/)
- [Go Redis Client](https://redis.uptrace.dev/guide/pubsub.html)
