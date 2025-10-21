# Streamario Integration Flow

## 概要
このドキュメントはStreamerioバックエンドの統合フローを説明します。

## システムフロー

### 1. ゲーム開始フロー
```
Unity起動
  ↓
WebSocket接続 (/ws-unity)
  ↓
ルームID発行 (ULID)
  ↓
DB登録 (rooms テーブル)
  ↓
QRコード/URL生成
  ↓
Unityへ通知 (room_created)
```

### 2. 視聴者参加フロー
```
視聴者がQRコードスキャン
  ↓
Webページアクセス
  ↓
viewer_id Cookie確認
  ↓
なし → 新規発行 (ULID)
あり → 既存ID使用
  ↓
DB登録 (viewers テーブル)
```

### 3. イベント送信フロー
```
視聴者がボタン押下
  ↓
POST /api/rooms/{id}/events
  body: {event_type, viewer_id}
  ↓
イベントDB保存 (events テーブル)
  ↓
視聴者アクティビティ更新 (Redis ZADD)
  ↓
カウント増加 (Redis INCR)
  ↓
閾値計算 (視聴者数 × 動的倍率)
  ↓
閾値到達？
  YES → Unity通知 (game_event)
        カウントリセット
  NO  → レスポンス返却
```

### 4. 閾値計算ロジック
```
視聴者数取得 (Redis ZCOUNT)
  ↓
倍率テーブル参照
  1-5人   → 1.0倍
  6-10人  → 1.2倍
  11-20人 → 1.5倍
  21-50人 → 2.0倍
  51人以上 → 3.0倍
  ↓
計算: base_threshold × multiplier
  ↓
上下限クランプ (min ~ max)
```

### 5. ゲーム終了フロー
```
Unity終了通知 OR POST /api/rooms/{id}/end
  ↓
DB集計クエリ実行
  - イベント×視聴者集計
  - イベント合計
  - 視聴者合計
  ↓
トップ算出
  - イベント別トップ
  - チーム別トップ (skill/enemy)
  - 全体トップ
  ↓
rooms.status = 'ended'
  ↓
Unityへサマリー送信 (game_end_summary)
  ↓
GET /api/rooms/{id}/results で結果取得可能
```

## WebSocketメッセージ仕様

### Unity → Backend
現在未使用（将来の拡張用）

### Backend → Unity

#### room_created
```json
{
  "type": "room_created",
  "room_id": "01HQ...",
  "qr_code": "https://...",
  "web_url": "https://..."
}
```

#### game_event (効果発動)
```json
{
  "type": "game_event",
  "event_type": "skill1",
  "trigger_count": 10,
  "viewer_count": 25
}
```

#### game_end_summary (終了サマリー)
```json
{
  "type": "game_end_summary",
  "top_by_button": {
    "skill1": {"viewer_id": "...", "viewer_name": "...", "count": 50}
  },
  "top_overall": {"viewer_id": "...", "viewer_name": "...", "count": 200},
  "team_tops": {
    "skill": {"viewer_id": "...", "count": 150},
    "enemy": {"viewer_id": "...", "count": 120},
    "all": {"viewer_id": "...", "count": 200}
  }
}
```

## 設定変更方法

### 管理API経由 (推奨)
```bash
# イベント閾値変更
curl -u admin:password -X PUT http://localhost:8888/admin/config/skill1 \
  -H "Content-Type: application/json" \
  -d '{"base_threshold": 8, "min_threshold": 5, "max_threshold": 60}'

# 視聴者倍率変更
curl -u admin:password -X PUT http://localhost:8888/admin/viewer-multipliers \
  -H "Content-Type: application/json" \
  -d '[
    {"max_viewers": 10, "multiplier": 1.0},
    {"max_viewers": 30, "multiplier": 1.5},
    {"max_viewers": 999999, "multiplier": 2.5}
  ]'

# 全設定確認
curl -u admin:password http://localhost:8888/admin/config
```

### 環境変数経由
```bash
# .env ファイルに追加
THRESHOLD_SKILL1_BASE=8
THRESHOLD_SKILL1_MIN=5
THRESHOLD_SKILL1_MAX=60
```

再起動が必要

## ログ出力例

### デバッグログ (LOG_LEVEL=debug)
```json
{
  "time": "2025-01-15T10:30:00Z",
  "level": "DEBUG",
  "msg": "Threshold calculated",
  "event_type": "skill1",
  "viewer_count": 25,
  "multiplier": 2.0,
  "raw": 10,
  "final": 10,
  "min": 3,
  "max": 50
}
```

### 効果発動ログ
```json
{
  "time": "2025-01-15T10:30:05Z",
  "level": "INFO",
  "msg": "🎯 Threshold reached - triggering effect",
  "room_id": "01HQ...",
  "event_type": "skill1",
  "current": 10,
  "threshold": 10
}
```

## エラーハンドリング

### データベースエラー
- 自動リトライなし
- エラーログ出力
- クライアントへ500エラー返却

### Redisエラー
- カウント失敗時: エラーログ + 処理中断
- 視聴者数取得失敗時: デフォルト値1で継続

### WebSocketエラー
- 送信失敗: ログ出力のみ、処理は継続
- 接続切断: 自動クリーンアップ

## パフォーマンス最適化

### DB接続プール
- Max Open: 25
- Max Idle: 5
- Max Lifetime: 5分

### Redis接続プール
- Pool Size: 10
- Min Idle: 2

### 視聴者アクティビティ
- 5分窓 (Redis ZSET)
- 古いデータ自動削除

## セキュリティ

### 管理API
- Basic認証必須
- 環境変数でクレデンシャル管理

### CORS
- フロントエンドオリジン限定
- 本番環境では`*`を削除推奨

### SQLインジェクション対策
- Prepared Statement使用
- sqlx自動エスケープ

## スケーリング

### 水平スケーリング
- ステートレス設計
- Redis共有でマルチインスタンス対応
- ロードバランサー配下に複数台配置可能

### 垂直スケーリング
- 接続プール設定調整
- メモリ割り当て増加

## トラブルシューティング

### 閾値が意図通りに変わらない
1. `GET /admin/config` で現在設定確認
2. ログで計算過程を確認 (DEBUG レベル)
3. 視聴者数が正しくカウントされているか確認

### Unityに通知が届かない
1. `GET /clients` で接続確認
2. WebSocketログ確認
3. room_id一致確認

### カウントがリセットされない
1. Redisログ確認
2. `redis-cli` で手動確認: `GET room:{id}:cnt:{type}`
3. 手動リセット: `DEL room:{id}:cnt:{type}`