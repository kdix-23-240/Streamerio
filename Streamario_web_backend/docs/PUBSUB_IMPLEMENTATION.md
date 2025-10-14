# Pub/Sub パッケージ実装ドキュメント

## 実装日時
2025-10-05

## 実装の背景と目的

### 課題
バックエンドをREST APIサーバーとWebSocketサーバーに分離してデプロイする際、以下の課題が発生：

1. **イベント配信の問題**: REST APIで受信したボタンイベントをUnity（WebSocket接続先）に通知する必要があるが、別プロセスになると直接呼び出しができない
2. **スケーラビリティ**: WebSocketサーバーを複数インスタンスで動かす場合、どのインスタンスに接続があるか不明
3. **疎結合性**: サーバー間の依存を最小限にし、独立してスケール可能にしたい

### 解決策
Redis Pub/Subを使った非同期メッセージング基盤を導入：
- REST APIサーバー: イベントをRedisに発行（Publish）
- WebSocketサーバー: Redisを購読（Subscribe）し、該当接続に配信
- 複数WebSocketサーバーがある場合も、全インスタンスがメッセージを受信し、自分が持つ接続に配信

## アーキテクチャ設計

### 全体フロー
```
[Frontend] 
    ↓ POST /api/rooms/{id}/events
[REST API Server]
    ↓ EventService.ProcessEvent()
    ↓ pubsub.Publish("game_events", event)
[Redis Pub/Sub]
    ↓ Broadcast to all subscribers
[WebSocket Server 1] ← Subscribe
[WebSocket Server 2] ← Subscribe
    ↓ SendEventToUnity() (if connection exists)
[Unity Client]
```

### コンポーネント分離
- **pkg/pubsub/**: Pub/Sub機能の抽象化レイヤー（本実装）
- **internal/service/event.go**: REST API側でPub/Sub経由でイベント発行
- **internal/handler/websocket.go**: WebSocketサーバー側でPub/Sub購読、Unity配信

## 実装の詳細

### ディレクトリ構造
```
pkg/pubsub/
├── interface.go      # 抽象インターフェース定義
├── redis.go          # Redis Pub/Sub実装（本番用）
├── memory.go         # インメモリ実装（開発/テスト用）
├── channels.go       # チャネル名定数定義
├── memory_test.go    # 単体テスト
└── README.md         # 使用方法ドキュメント
```

### 設計原則

#### 1. 高凝集・低結合
- **インターフェース分離**: `PubSub`インターフェースで抽象化し、実装の詳細を隠蔽
- **依存性注入**: サービス層はインターフェースに依存し、具体的な実装は起動時に注入
- **交換可能性**: 環境に応じてRedis/Memory実装を切り替え可能

既存の`pkg/counter/`パッケージと同じ構造を踏襲し、コードベース全体の一貫性を保持。

#### 2. エラーハンドリング
- **Publish失敗**: Redis接続エラー時はエラーを返す（呼び出し側でリトライ判断）
- **Subscribe失敗**: 購読開始失敗時はエラーを返し、contextキャンセル時は正常終了
- **Handler内エラー**: ログ記録のみ、購読は継続（一部メッセージの失敗で全体停止しない）

#### 3. 並行処理の安全性
- **Memory実装**: sync.RWMutexで購読者リストを保護
- **Redis実装**: Redis Clientが内部で並行安全を保証
- **Context管理**: context.Contextで購読のライフサイクルを制御

### 実装の工夫

#### 1. ログ出力の一貫性
既存の`pkg/counter/redis.go`と同じスタイルで統一：
- 操作ごとにloggerを作成（op, channel, room_idなど付与）
- 処理時間（elapsed）を計測してログ出力
- Debugレベル: 正常系の詳細情報
- Errorレベル: 異常系とハンドラエラー

#### 2. メモリ実装の実用性
開発環境でRedisが不要な場合を考慮：
- チャネルバッファ（100）でバースト対応
- 購読者が増減しても安全に動作（defer cleanup）
- Closeで全リソースを確実に解放

#### 3. テスタビリティ
- インターフェース化により、モック作成が容易
- Memory実装でRedis不要のテストが可能
- context.WithTimeoutでテストのタイムアウト制御

## 使用方法

### REST APIサーバー側（Publish）

#### 1. 初期化（main.go）
```go
// Redis Client (既存のものを再利用)
rdb := redis.NewClient(opt)

// PubSub初期化
ps := pubsub.NewRedisPubSub(rdb, appLogger.With(slog.String("component", "pubsub")))

// EventServiceに注入
eventService := service.NewEventService(
    redisCounter, 
    eventRepo, 
    sender, // 既存のWebSocketSender（後で置き換え）
    eventLogger,
)
```

#### 2. EventServiceでの使用
```go
// ProcessEvent内で閾値到達時
if int(current) >= threshold {
    payload := map[string]interface{}{
        "type": "game_event",
        "room_id": roomID,
        "event_type": string(eventType),
        "trigger_count": int(current),
        "viewer_count": viewers,
    }
    
    message, _ := json.Marshal(payload)
    if err := s.pubsub.Publish(ctx, pubsub.ChannelGameEvents, message); err != nil {
        s.logger.Error("event publish failed", slog.Any("error", err))
    }
}
```

### WebSocketサーバー側（Subscribe）

#### 1. 購読開始（main.go）
```go
// メッセージハンドラを定義
handler := func(channel string, message []byte) error {
    var payload map[string]interface{}
    if err := json.Unmarshal(message, &payload); err != nil {
        return err
    }
    
    roomID, _ := payload["room_id"].(string)
    
    // WebSocketHandlerで配信
    return wsHandler.SendEventToUnity(roomID, payload)
}

// 購読開始（別goroutine推奨、定数を使用）
go func() {
    ctx := context.Background()
    if err := ps.Subscribe(ctx, pubsub.ChannelGameEvents, handler); err != nil {
        log.Error("subscription failed", slog.Any("error", err))
    }
}()
```

## チャネル設計

チャネル名は `pkg/pubsub/channels.go` で定数定義されています。

### `pubsub.ChannelGameEvents`
ボタンイベントの閾値到達通知

**実際の文字列**: `"game_events"`  
**用途**: REST API → WebSocket → Unity  
**Payload**:
```json
{
  "type": "game_event",
  "room_id": "01HXXX...",
  "event_type": "skill1",
  "trigger_count": 5,
  "viewer_count": 12
}
```

### `pubsub.ChannelGameEnd`（将来拡張用）
ゲーム終了通知

**実際の文字列**: `"game_end_notifications"`  
**用途**: WebSocket → REST API（統計更新など）  
**Payload**:
```json
{
  "type": "game_end",
  "room_id": "01HXXX...",
  "ended_at": "2025-10-05T12:00:00Z"
}
```

## テスト戦略

### 単体テスト（memory_test.go）
- ✅ 基本的なPublish/Subscribe動作
- ✅ 複数購読者への同時配信
- ✅ 購読者がいない場合の動作
- ✅ Closeによるリソース解放

### 統合テスト（今後実装予定）
- Redis実装での実際のPub/Sub動作
- 複数プロセス間でのメッセージ配信
- ネットワークエラー時の挙動

### E2Eテスト（手動検証）
- REST APIでイベント送信 → Unity側で受信確認
- 複数WebSocketサーバーでの負荷分散
- サーバー再起動時の再接続

## パフォーマンス考慮

### レイテンシ
- **Publish**: Redis PUBLISHコマンド（通常 < 1ms）
- **配信**: Subscribe側がノンブロッキング受信、ハンドラ並列実行可能
- **トータル**: REST API受信 → Unity配信まで数ms〜十数ms（ネットワーク次第）

### スループット
- Redis Pub/Subは数万msg/sec対応可能
- 本アプリケーションの想定負荷（数十〜数百msg/sec）なら余裕

### リソース使用
- **Memory実装**: チャネルバッファで数KB程度
- **Redis実装**: 接続1本のみ（購読チャネルは内部で多重化）

## 今後の拡張計画

### 短期（v1リリース前）
- [ ] EventServiceへのPubSub統合
- [ ] WebSocketサーバーでのSubscribe実装
- [ ] ローカル環境での動作確認

### 中期（v2）
- [ ] Redis Streamsへの移行検討（永続化、履歴取得）
- [ ] メトリクス収集（発行数、レイテンシ）
- [ ] リトライ機構（一時的なRedis接続断への対応）

### 長期（v3以降）
- [ ] パターンマッチ購読（PSUBSCRIBE）
- [ ] メッセージ圧縮（大きなペイロード対応）
- [ ] 優先度付きキュー（重要イベントを優先処理）

## 参考資料
- [Redis Pub/Sub公式ドキュメント](https://redis.io/docs/manual/pubsub/)
- [Go Redis Client - Pub/Sub](https://redis.uptrace.dev/guide/pubsub.html)
- 既存実装: `pkg/counter/redis.go`（ログ・エラーハンドリングの参考）

## まとめ

### 実装のポイント
1. **抽象化**: インターフェースで実装を隠蔽し、交換可能に
2. **一貫性**: 既存の`pkg/counter`と同じ構造・スタイルで統一
3. **実用性**: 本番（Redis）と開発（Memory）両方をサポート
4. **拡張性**: チャネル追加、実装切り替えが容易

### メリット
- REST APIとWebSocketを独立してデプロイ・スケール可能
- サーバー間の結合度が低く、一方の変更が他方に影響しにくい
- Redisが単一障害点になるが、既にカウンタでも使用しており新たな依存は増えない

### トレードオフ
- 同期的なエラーハンドリングが難しい（PublishしてもUnityに届いたか不明）
- Redis追加コスト（Upstash無料枠で対応可能）
- デバッグが若干複雑（メッセージフローの追跡）

本実装により、バックエンドの分離デプロイが可能になり、スケーラビリティと保守性が向上します。
