# Streamerio 統合フロー / 実装ガイド

## 1. 全体像
Streamerio の最小構成は以下の 3 レイヤで構成されます。

1. WebSocket (Unity クライアント向け)  : ルームID = WebSocket 接続ID (ULID) 生成と双方向イベント配送
2. REST API (視聴者操作エンドポイント): ボタン押下イベント送信 / 統計取得
3. 内部サービス / 永続化: PostgreSQL (rooms / events), Redis (カウンタ & アクティブ視聴者ウィンドウ)

イベントが閾値に達すると WebSocket で Unity へ `game_event` を push し、カウンタをリセットします。

## 2. シーケンス (時系列)
```
Unity ----(WS接続)----> /ws-unity
  1. サーバ側: 接続登録 / ULID発行
  2. サーバ -> Unity: {type: "room_created", room_id: <ULID>} を送信

Frontend(Viewer) --- POST /api/rooms/{room_id}/events ---> Backend
  3. API: EnsureRoom (存在しなければ作成: anonymous 所有)
  4. EventService.ProcessEvent:
       a. DB(events) に INSERT
       b. Redis カウンタ increment
       c. viewer activity 更新 (ZSET - 5分窓)
       d. 動的閾値計算 (BaseThreshold × viewerMultiplier)
       e. 閾値到達なら WebSocket push → カウンタ reset

Frontend/View UI --- GET /api/rooms/{room_id}/stats ---> Backend
  5. 現在カウント/閾値/視聴者数を返す
```

## 3. 動的閾値算出ロジック概要
- 定義: `BaseThreshold` をベースに、アクティブ視聴者数から multiplier を決定
- multiplier テーブル例:
  - 1〜5: 1.0
  - 6〜10: 1.2
  - 11〜20: 1.5
  - 21〜50: 2.0
  - 51+: 3.0
- 計算後: `ceil(Base * mult)` を `MinThreshold`〜`MaxThreshold` で clamp
- 閾値到達時: カウンタ reset → 次の閾値を再計算

## 4. エンドポイント / 呼び出し仕様
### 4.1 WebSocket
- URL: `ws://<host>/ws-unity`
- 接続直後サーバ送信:
```json
{
  "type": "room_created",
  "room_id": "01HXXXX...",  
  "qr_code": "data:image/png;base64,...", 
  "web_url": "https://example.com"
}
```
- イベント発火時サーバ送信 (閾値到達):
```json
{
  "type": "game_event",
  "event_type": "help_speed",
  "trigger_count": 5,
  "viewer_count": 12
}
```

### 4.2 REST API
| Method | Path | Description |
|--------|------|-------------|
| GET | `/api/rooms/{room_id}` | ルーム情報取得（現在は EnsureRoom で暗黙作成後返す想定に変更可） |
| POST | `/api/rooms/{room_id}/events` | 視聴者イベント送信 (body: event_type, viewer_id) |
| GET | `/api/rooms/{room_id}/stats` | 現在の各イベントカウンタと閾値状況 |

#### リクエスト例 (イベント送信)
```bash
curl -X POST http://localhost:8888/api/rooms/01HXXXX.../events \
  -H 'Content-Type: application/json' \
  -d '{"event_type":"help_speed","viewer_id":"viewer123"}'
```

#### レスポンス例 (イベント送信)
```json
{
  "event_type": "help_speed",
  "current_count": 1,
  "required_count": 5,
  "effect_triggered": false,
  "viewer_count": 1,
  "next_threshold": 5
}
```

#### レスポンス例 (閾値到達時)
```json
{
  "event_type": "help_speed",
  "current_count": 0,             // reset 後
  "required_count": 5,
  "effect_triggered": true,
  "viewer_count": 8,
  "next_threshold": 6
}
```

#### 統計取得レスポンス例
```json
{
  "room_id": "01HXXXX...",
  "stats": [
    {
      "event_type": "help_speed",
      "current_count": 1,
      "current_level": 1,
      "required_count": 5,
      "next_threshold": 5,
      "viewer_count": 1
    }
  ],
  "time": "2025-09-19T18:12:41.869385+09:00"
}
```

## 5. 内部主要コンポーネントと役割
| ファイル | 役割 |
|----------|------|
| `internal/handler/websocket.go` | WebSocket 接続管理 / 送信 (`SendEventToUnity`) |
| `internal/handler/api.go` | REST ハンドラ (`SendEvent`, `GetRoomStats`) |
| `internal/service/event.go` | ビジネスロジック（記録・閾値計算・通知・リセット） |
| `internal/service/room.go` | ルーム存在確認・生成 (`EnsureRoom`, `GenerateRoom`) |
| `internal/repository/event.go` | DB `events` INSERT |
| `internal/repository/room.go` | DB `rooms` CRUD (必要最小) |
| `pkg/counter/redis.go` | ルーム×イベント種別カウント + アクティブ視聴者 ZSET |

## 6. 関数呼び出しチェーン詳細
### 6.1 イベント送信 (POST /events)
```
APIHandler.SendEvent
  └─ RoomService.EnsureRoom (存在しなければ rooms INSERT)
  └─ EventService.ProcessEvent
       ├─ EventRepository.CreateEvent (DB INSERT)
       ├─ Counter.UpdateViewerActivity (ZADD + 過去掃除)
       ├─ Counter.Increment
       ├─ EventService.getActiveViewerCount (Counter.GetActiveViewerCount)
       ├─ 閾値計算 (calculateDynamicThreshold)
       ├─ (条件) WebSocketSender.SendEventToUnity
       └─ Counter.Reset (発動時)
```

### 6.2 統計取得 (GET /stats)
```
APIHandler.GetRoomStats
  └─ RoomService.EnsureRoom
  └─ EventService.GetRoomStats
       ├─ Counter.Get (各 eventType)
       ├─ getActiveViewerCount
       └─ calculateDynamicThreshold
```

## 7. フロントエンド統合ステップ（推奨フロー）
1. Unity クライアント起動時に WebSocket 接続 → `room_id` 取得
2. Web UI / 視聴者側はその `room_id` を共有（URL / QR など）
3. 視聴者操作でイベント API へ POST
4. (オプション) 一定間隔 (例 3〜5s) で `/stats` ポーリング or SSE/WS 拡張
5. 閾値到達通知 (`game_event`) を Unity は受信し、ゲーム内エフェクトを再生
6. Unity 側は必要あればカウントリセット後の状況を表示更新（ただし通知に直近情報含むため即反映可能）

## 8. ルーム生成戦略
| 戦略 | 説明 | 長所 | 短所 |
|------|------|------|------|
| WebSocket ULID = RoomID (現行) | 接続毎に新規 | シンプル | 再接続でID変化 |
| 事前発行 API | `/api/rooms` (未実装) で事前発行 | 再接続安定 | 実装追加必要 |
| DB 永続セッション ID | 履歴/分析向け | 分析容易 | 要スキーマ拡張 |

## 9. エッジケース & ハンドリング
| ケース | 現状挙動 | 改善案 |
|--------|----------|--------|
| WebSocket 未接続で閾値到達 | ログにエラー (送信失敗) | リトライキュー化 |
| viewer_count = 0 | 1 にフォールバック | 0 のままスキップオプション |
| 同時多発 POST | Redis INCR で整合 | ロック不要 / OK |
| 過去カウンタ持越し | 残る | 配信開始 API でリセット |
| 閾値激増 (大量視聴者) | 上限 clamp | 動的調整 UI 提供 |

## 10. 追加したい拡張アイデア（任意）
- Reset API: `POST /api/rooms/{room_id}/reset` で全イベントカウンタクリア
- WebSocket KeepAlive: ping/pong で接続死活検出
- イベント種別の有効化/無効化設定 (rooms.settings JSONB)
- 閾値到達履歴テーブル (今は送信時点のみログ)

## 11. フロント/Unity 実装サンプル
### WebSocket (ブラウザ JS)
```js
const ws = new WebSocket('ws://localhost:8888/ws-unity');
ws.onmessage = ev => {
  const msg = JSON.parse(ev.data);
  if (msg.type === 'room_created') {
    console.log('ROOM ID', msg.room_id);
  } else if (msg.type === 'game_event') {
    console.log('TRIGGER', msg.event_type, msg.trigger_count);
  }
};
```

### Unity (C# pseudo)
```csharp
var ws = new WebSocket("ws://localhost:8888/ws-unity");
ws.OnMessage += (sender, e) => {
  var json = JsonUtility.FromJson<WsMessage>(e.Data);
  if (json.type == "room_created") RoomId = json.room_id;
  if (json.type == "game_event") TriggerEffect(json.event_type);
};
ws.Connect();
```

### イベント送信 (ブラウザ fetch)
```js
await fetch(`http://localhost:8888/api/rooms/${roomId}/events`, {
  method: 'POST',
  headers: {'Content-Type':'application/json'},
  body: JSON.stringify({event_type:'help_speed', viewer_id:'user123'})
});
```

## 12. 既知の制約
- 認証/認可は未実装 (任意の room_id で投稿可能)
- ルーム有効期限 (TTL) は現状ロジックに存在しない（将来拡張予定）
- WebSocket 停止中のトリガーはロスト（再送なし）

## 13. 早見表: 呼び出すべき主関数
| レイヤ | 関数 | 目的 |
|--------|------|------|
| WebSocket | `WebSocketHandler.HandleUnityConnection` | Unity からの接続受理 + room_id 付与 |
| API | `APIHandler.SendEvent` | 視聴者イベント受付 / ルーム自動生成 |
| API | `APIHandler.GetRoomStats` | 閾値・カウント確認 |
| Service | `EventService.ProcessEvent` | 1イベント処理主本体 |
| Service | `EventService.GetRoomStats` | 集計取得 |
| Service | `RoomService.EnsureRoom` | ルーム存在保証 |
| Counter | `Counter.Increment` | イベントカウント加算 |
| Counter | `Counter.UpdateViewerActivity` | 視聴者アクティビティ登録 |

---
以上。改善や追加が必要であれば指示をください。次に追加するなら Reset API / 認証 / WebSocket 再送あたりが候補です。
