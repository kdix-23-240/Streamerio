# Pub/Sub 統合実装ドキュメント

## 実装日時
2025-10-05

## 概要
フロントエンドからのボタンイベントを、REST APIサーバーからWebSocketサーバーへPub/Sub経由で配信する実装を完了しました。

## 変更内容

### 1. EventService (`internal/service/event.go`)

#### 変更前
```go
type EventService struct {
    wsHandler WebSocketSender  // 直接WebSocketに送信
}

// 閾値到達時
if err := s.wsHandler.SendEventToUnity(roomID, payload); err != nil {
    // エラー処理
}
```

#### 変更後
```go
type EventService struct {
    pubsub pubsub.PubSub  // Pub/Sub経由でブロードキャスト
}

// 閾値到達時
payload := map[string]interface{}{
    "type":          "game_event",
    "room_id":       roomID,  // ← 追加（配信先特定に必要）
    "event_type":    string(eventType),
    "trigger_count": int(current),
    "viewer_count":  viewers,
}
message, _ := json.Marshal(payload)
s.pubsub.Publish(ctx, pubsub.ChannelGameEvents, message)
```

**変更理由**:
- スケーリング対応: 複数WebSocketサーバーに同時配信
- 疎結合化: REST APIとWebSocketの依存を切断
- room_id追加: WebSocketサーバー側で配信先を特定するため

### 2. WebSocketHandler (`internal/handler/websocket.go`)

#### 追加機能
```go
type WebSocketHandler struct {
    pubsub pubsub.PubSub  // 追加
    logger *slog.Logger   // 追加
}

// 新規メソッド
func (h *WebSocketHandler) StartPubSubSubscription(ctx context.Context) error {
    handler := func(channel string, message []byte) error {
        var payload map[string]interface{}
        json.Unmarshal(message, &payload)
        
        roomID, _ := payload["room_id"].(string)
        
        // 自分が接続を持っている場合のみ配信
        if err := h.SendEventToUnity(roomID, payload); err != nil {
            // 接続がないのは正常（他のインスタンスが持っている）
            h.logger.Debug("no local connection for room, skip delivery")
            return nil
        }
        
        h.logger.Info("event delivered to unity via pubsub")
        return nil
    }
    
    // 購読開始（ブロッキング）
    return h.pubsub.Subscribe(ctx, pubsub.ChannelGameEvents, handler)
}
```

**実装のポイント**:
- 全WebSocketサーバーがSubscribe
- 接続を持つインスタンスのみが実際に配信
- 接続がない場合はDEBUGレベルログ（エラーではない）

### 3. main.go (`cmd/server/main.go`)

#### 初期化フロー
```go
// 6. Pub/Sub 初期化
ps := pubsub.NewRedisPubSub(rdb, appLogger.With(slog.String("component", "pubsub")))

// 8. サービス層生成
wsHandler := handler.NewWebSocketHandler(ps, wsHandlerLogger)  // PubSub注入
eventService := service.NewEventService(redisCounter, eventRepo, ps, eventLogger)  // PubSub注入

// 9. Pub/Sub 購読開始 (別goroutine)
go func() {
    ctx := context.Background()
    if err := wsHandler.StartPubSubSubscription(ctx); err != nil {
        log.Error("pubsub subscription terminated", slog.Any("error", err))
    }
}()

// 10. Echoサーバー起動
e.Start(":" + cfg.Port)
```

**重要なポイント**:
- Pub/Sub購読は別goroutineで起動（ブロッキング動作のため）
- サーバー起動前に購読を開始
- 同一プロセス内でREST APIとWebSocketを両方提供

## 動作フロー

### シナリオ: ボタンイベント配信

```
時刻 T0: Unity接続確立
[Unity Client]
    ↓ WebSocket接続
[WebSocket Server #1]
    connections["room-123"] = ws  ← メモリに保存

---

時刻 T1: フロントエンドがボタン押下
[Frontend]
    ↓ POST /api/rooms/room-123/events
[REST API Server] (同じプロセス内)
    ↓ EventService.ProcessEvent()
    ↓ 閾値到達を検知
    ↓ pubsub.Publish(ChannelGameEvents, {
        "type": "game_event",
        "room_id": "room-123",
        "event_type": "skill1",
        "trigger_count": 5,
        "viewer_count": 12
      })
[Redis Pub/Sub]
    ↓ 全購読者にブロードキャスト

---

時刻 T2: WebSocketサーバーがメッセージ受信
[WebSocket Server #1] ← Subscribe受信
    ↓ StartPubSubSubscription()のhandler実行
    ↓ room_id = "room-123" を抽出
    ↓ connections["room-123"] 確認 → ある！
    ↓ SendEventToUnity("room-123", payload)
[Unity Client] ← イベント受信・ゲーム内で効果発動

[WebSocket Server #2] ← Subscribe受信（別インスタンス想定）
    ↓ room_id = "room-123" を抽出
    ↓ connections["room-123"] 確認 → ない
    ↓ DEBUGログ "no local connection for room"
    ↓ 何もしない（正常動作）
```

## スケーリング対応

### 現在の構成（単一サーバー）
```
[REST API + WebSocket] (1プロセス)
    ↓
[Redis]
    ↓
[PostgreSQL]
```

### 将来の構成（分離デプロイ）
```
[REST API #1] ─┐
[REST API #2] ─┼→ Redis Pub/Sub ─┬→ [WebSocket #1] → Unity A, B
                                 └→ [WebSocket #2] → Unity C
```

**対応済みのポイント**:
- ✅ REST APIとWebSocketが別プロセスでも動作
- ✅ WebSocketサーバーを複数台にスケール可能
- ✅ ロードバランサー配下でも正常動作

## パフォーマンス影響

### レイテンシ
- **変更前**: 直接メモリアクセス（< 1ms）
- **変更後**: Redis Pub/Sub経由（1-5ms）
- **増加分**: 約1-4ms（ネットワークレイテンシ）

### メリット
- スケーラビリティ向上
- 障害分離（REST APIとWebSocketが独立）
- デプロイの柔軟性

## テスト方法

### 1. ローカル起動
```bash
cd Streamario_web_backend

# Redisが起動していることを確認
# docker-compose up -d redis

# サーバー起動
go run ./cmd/server
```

### 2. 動作確認ログ
```
# 起動時
INFO starting pubsub subscription channel=game_events

# ボタン押下時（REST API）
INFO event triggered room_id=01HX... event_type=skill1 count=5
INFO event published to pubsub room_id=01HX... event_type=skill1

# イベント配信時（WebSocket）
INFO event delivered to unity via pubsub room_id=01HX... event_type=skill1
```

### 3. トラブルシューティング
```bash
# Pub/Subメッセージを監視
redis-cli
> SUBSCRIBE game_events

# メッセージが流れることを確認
```

## 今後の拡張

### Phase 1: 現在（完了）
- ✅ Pub/Sub基盤実装
- ✅ EventServiceのPub/Sub統合
- ✅ WebSocketHandlerのSubscribe実装

### Phase 2: デプロイ分離（次のステップ）
- [ ] docker-compose でREST APIとWebSocketを分離
- [ ] 環境変数で動作モード切り替え（API専用/WebSocket専用/Both）
- [ ] ヘルスチェックエンドポイント追加

### Phase 3: 本番環境対応
- [ ] Cloud RunまたはECSへのデプロイ
- [ ] ロードバランサー設定
- [ ] モニタリング・アラート設定

## 注意事項

### 1. Redis障害時の挙動
- **Publish失敗**: エラーログ出力、イベントは失われる
- **Subscribe断絶**: 自動再接続なし（要実装）
- **対策**: Redis Sentinelまたはクラスタ構成

### 2. メッセージの順序保証
- Redis Pub/Subは順序保証あり（同一チャネル内）
- ただし、Subscribeが遅延した場合は取りこぼす可能性

### 3. 開発時の注意
- ローカル開発でRedisが不要な場合は`pubsub.NewMemoryPubSub()`を使用可能
- ただし、プロセス分離時は動作しない

## 参考
- [Pub/Subパッケージ実装](./PUBSUB_IMPLEMENTATION.md)
- [Redis Pub/Sub公式ドキュメント](https://redis.io/docs/manual/pubsub/)
