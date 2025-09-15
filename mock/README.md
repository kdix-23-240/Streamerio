# シンプルWebアプリ(Next + Go + HTML)

構成:

- `backend/` Go製バックエンド
  - `POST /trigger` を受けると、`/ws` に接続中の全クライアントへWebSocketで通知
- `frontend-next/` Next.js製フロント(ボタンでPOST送信)
- `simple-client/` プレーンHTML製の受信ページ(WebSocket接続)

## Go の環境構築（未経験者向け）

- 推奨バージョン: `Go 1.20` 以上
- 公式ダウンロード: https://go.dev/dl/

インストール方法（例）
- macOS: `brew install go`（Homebrew）または公式PKG
- Windows: 公式MSIインストーラを実行
- Linux: 公式tar.gzを `/usr/local` に展開、またはディストロのパッケージを使用

確認コマンド
```
go version
go env
```

PATH 設定の目安
- macOS/Linux: `/usr/local/go/bin` を `PATH` に含める
- Windows: インストーラが自動でPATHを設定

補足
- 本バックエンドは標準ライブラリのみで動作します（追加依存なし）。
- Go Modulesはデフォルト有効。任意で `go mod init example` をしてもOKです。

## 起動方法

前提: ローカルでポート `8080` と `3000` が空いていること。

### 1) Go バックエンド

```
cd backend
go run ./
```

起動すると `:8080` で待ち受けます。

エンドポイント:
- WebSocket: `ws://localhost:8080/ws`
- Trigger(POST): `http://localhost:8080/trigger`

`/trigger` はCORS対応済み(簡易)です。

### 2) Next フロント(ボタン送信側)

別ターミナルで:

```
cd frontend-next
npm install
npm run dev
```

ブラウザで `http://localhost:3000` を開き、ボタン「命令を送る」をクリックすると、バックエンドへ `POST /trigger` が飛びます。

### 3) シンプルHTMLクライアント(受信側)

`simple-client/index.html` をブラウザで直接開いてください(ファイルをダブルクリック、またはブラウザにドラッグ&ドロップ)。

ページが `ws://localhost:8080/ws` へ接続し、メッセージをログに表示します。

### 動作確認フロー

1. バックエンドを起動
2. `simple-client/index.html` を開いて「接続しました」と表示されることを確認
3. Next を起動して `http://localhost:3000` を開く
4. 「命令を送る」を押す
5. シンプルHTML側に「命令が来たよ: Nextからの命令」等が表示される

## 実装メモ

- GoのWebSocketは外部ライブラリを使わず、HTTPハイジャック＋最小限のフレーミングで実装しています(デモ用途)。
- CORSは `/trigger` のみ簡易対応(Originは `*` 許可)。
- WebSocketはCORS対象外のため、ファイル直開きのHTMLからでも接続できます。
