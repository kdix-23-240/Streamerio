# Repository Guidelines

## プロジェクト構成 & モジュール配置
- `frontend/` Next.js + TypeScript（`src/app`, `src/components`, `src/lib`）
- `Streamario_web_backend/` Go (Echo)（`cmd/server`, `internal/...`, `pkg/...`, `db/migrations`）
- `Streamerio_unity/` Unity（`Assets/0_Scripts` ほか）
- ルートに `docker-compose.yml`（API/DB 開発用）

## ビルド・テスト・ローカル実行
- Frontend（Node 20）
  - 初回: `cd frontend && npm ci`
  - 開発: `npm run dev`（http://localhost:3000）
  - 本番: `npm run build` / Lint: `npm run lint`
  - 環境: `frontend/.env.example` を複製
- Backend（Go 1.25）
  - 初回: `cd Streamario_web_backend && cp .env.example .env && go mod download`
  - 実行: `go run ./cmd/server`（デフォルト `:8888`）/ ビルド: `go build ./cmd/server`
  - Docker: ルートで `docker-compose up --build`（Postgres 同時起動）
- Unity
  - Unity Hub で `Streamerio_unity` を開き、Play 実行

## コーディングスタイル & 命名
- Frontend: 2スペース、ESLint。React コンポーネント= `PascalCase`、変数= `camelCase`。`src/components` はファイル名も `PascalCase.tsx` を推奨。Tailwind はユーティリティ優先。
- Backend: `gofmt`/`go vet` 準拠。パッケージ名=小文字、公開関数= `PascalCase`。エラーは `errors` でラップ。設定は環境変数（`.env`）。
- Unity(C#): クラス/メソッド= `PascalCase`、フィールド= `camelCase`。`Assets/0_Scripts` 規約に従う。

## テスト方針
- Go: 作成時に `_test.go` を配置し、`go test ./...` を実行。
- Frontend: 現状は Lint を最低限のゲートとし、将来的に Vitest/Jest を検討。
- PR 前にローカルで Lint/Test を必ず実行。

## コミット & Pull Request
- Conventional Commits を推奨（例: `feat(frontend): add button grid component`）。
- PR には以下を含める: 目的/背景、変更点、動作確認手順、スクリーンショット（UI 変更時）、関連 Issue、移行/設定の注意（`.env`, DB など）。

## セキュリティ & 設定
- 秘密情報はコミット禁止。`.env.example` を複製して使用。
- 本番は CORS の許可元を限定。Redis（Upstash 等）は `REDIS_URL` を環境変数で指定。

## エージェント/自動化向け注意
- 本ファイルのスコープはリポジトリ全体。配下に別の `AGENTS.md` がある場合は、より深い階層を優先。
- 変更は最小限かつ目的限定。無関係な修正や大規模リネームは避ける。
- 既存のスタイル/命名規約に合わせ、必要に応じてドキュメントを更新。
game_end_plan.mdに従いながら実装をしてください
- 開発を進めるたびにこの実装にした意図などをマークダウン形式にどこかのファイルにまとめて書いておいてください
- 使いづらくならない程度で高凝集低結合を意識してください
- 現状の実装から大きくずれないように書いてください
