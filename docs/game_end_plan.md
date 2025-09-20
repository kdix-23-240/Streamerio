# ゲーム終了処理 実装計画

## 1. 現状把握と前提整理
- Backend は `internal/handler/websocket.go` で Unity との WebSocket、`EventService` がボタンイベント処理。
- Frontend は `src/app/page.tsx` が視聴者向け操作 UI。終了時 UI は未実装。
- DB の `events.event_type` は `skill1`〜`skill3`、`enemy1`〜`enemy3` に統一済み。`rooms.status` でルーム状態を管理。
- Redis はボタン別カウンタとアクティブ視聴者のみ。終了集計には DB 利用が前提。

## 2. 終了状態モデルの設計
- ルームに「終了」を示す状態（例: `status = ended`）と `ended_at` などのタイムスタンプを追加する。
- 終了時に保持すべき情報: ボタン種別別集計、viewer 別集計、`skill1`〜`skill3` / `enemy1`〜`enemy3` ごとの最多押下 viewer、および全体トップ。
- 再利用可能にするため、終了結果を保存するテーブル/JSON カラムやキャッシュ構造を検討。

## 3. Unity からの終了イベント受信フロー
- `HandleUnityConnection` の受信ループで受信 JSON を解析し、`type: "game_end"` を検知したら新設の `GameSessionService.EndGame(roomID, payload)` を呼ぶ。
- Unity 側が送る roomID（初回発行 ID を再利用）と追加メタ情報の仕様を定義。
- 重複終了リクエストや room 未登録時の挙動（早期 return + ログなど）を決める。

## 4. 終了集計ロジック
- `events` テーブルから `viewer_id` × `event_type`（`skill1` など）の `COUNT(*)` 集計クエリを用意。
- 各イベント種別で最多押下 viewer、および全イベント合算の最多 viewer を算出。タイがある場合の優先順位を定義。
- 集計結果を Unity/フロント双方で使える JSON 形式にまとめる。
- 終了後に不要となる Redis カウンタや viewer activity の処理（削除 or TTL）を決める。

## 5. Backend API／WebSocket 拡張
- WebSocket で Unity へ送る終了通知フォーマットを設計（例: `{"type":"game_end_summary","top_by_button":{...},"top_overall":...}`）。
- REST API: `POST /rooms/{id}/events` に終了済みチェックを追加し、終了後リクエストには `game_over: true` と viewer の累計情報（`skill*/enemy*` 別）を返す。
- 視聴者リザルト表示用 `GET /rooms/{id}/results` を追加し、全体ランキング・viewer 個別統計・終了時刻などを返却。

## 6. Frontend 対応
- 新ページ `app/result/[roomId]/page.tsx`（仮）で終了メッセージ、ランキング、個人サマリーを表示。
- `lib/api.ts` に終了レスポンス処理と結果取得 API クライアントを追加。
- 操作パネル (`page.tsx`) は `game_over: true` を受け取ったら結果ページへ誘導し、モーダル等で終了を通知。

## 7. テスト & 検証
- Go: 終了集計ロジックと API ハンドラの単体テスト（`httptest`）。
- Frontend: 終了レスポンスでの遷移・表示の検証（React Testing Library など）。
- 手動検証: モック WebSocket クライアントで `game_end` を送信し、一連の流れを確認。

## 8. 移行 & ドキュメント
- 追加マイグレーション（`rooms.status` 更新や新テーブル）を作成し、`docker-compose` で適用手順を明記。
- `.env` に必要な設定を追記（例: 終了結果キャッシュ TTL）。
- `README` や `AGENTS.md` に終了フローと新エンドポイントを追加。

## 9. 未決定事項（要確認）
- Unity → Backend の終了通知 payload 詳細（roomID、終了理由、スコアなど）。
- `skill*/enemy*` の呼称を UI 上でどう表示するか（ラベルの文言、ローカライズ）。
- 同着トップが複数いる場合の扱い（最初の viewer だけ返すか、複数返すか）。
- 終了後にイベントを受け付ける猶予の有無と扱い。
- リザルトページの公開範囲（roomID を知っていれば閲覧可か、viewer ごとに制限するか）。
