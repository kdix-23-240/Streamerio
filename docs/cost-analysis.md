# ストリームシステム コスト検討メモ

## 現行構成
- バックエンド: Go (Echo) を Cloud Run Always Free 枠で稼働。
- データベース: Supabase (Postgres)。
- キャッシュ/カウンタ: Upstash Redis。
- フロントエンド: Next.js (Vercel)。
- ゲームクライアント: Unity を UnityRoom へデプロイ。

## Cloud Run の課金発生タイミング
- Always Free 枠上限: 月 200 万 vCPU 秒、100 万 GiB 秒、リクエスト 200 万、外向け転送 5GB。
- 枠超過後の単価: vCPU 秒あたり約 $0.000024、GiB 秒あたり約 $0.0000025、リクエスト 100 万毎に $0.40（us-central1 参考値）。
- 秒間 50 ユーザー×5 リクエスト、処理 200ms と仮定: 月 13 万 vCPU 秒・6.5 万 GiB 秒程度で無料枠内に収束。
- `min-instances` を 1 以上に上げる、CPU 常時割当を有効化する場合はアイドル時間でも課金される点に注意。

## Supabase / Upstash のコストしきい値
- Supabase Free: DB/ストレージ各 500MB、帯域 10GB/月、匿名ユーザー 10 万人/月。容量超過や帯域超過で Pro ($25/月 + 従量) が必要。
- Upstash Redis Free: ストレージ 1GB、10K コマンド/日目安。コマンド超過時は Pro ($20/月〜)。
- Redis の使用量が増える場合は、一括更新や TTL でコマンド削減を検討。

## 規模拡大時の選択肢
- Cloud Run の vCPU/メモリを段階的に増強しつつ、`max-instances` を調整してスパイク吸収。
- 集計処理は Pub/Sub + Cloud Run Jobs などで非同期化し、個別コンテナの CPU 滞留を避ける。
- 常時高負荷なら GKE Autopilot への移行を検討。ただし運用コスト・管理負荷が上がる。

## 言語・基盤リプレースによるコスト影響
- Go は起動が速くメモリ効率が高いため Cloud Run 課金を抑制しやすい。Node.js/Python への移行はコールドスタートやメモリ使用増で逆効果になる可能性が高い。
- さらに効率を求めて Rust/Elixir 等へ移る場合は開発コスト・チーム習熟とのトレードオフが大きい。
- Supabase から Cloud SQL への移行は最小構成でも $0.15/時程度かかるため、まずは Supabase Pro へのアップグレードが現実的。

## 負荷テストのコスト要素
- OSS ツール (k6, Locust, JMeter) 自体は無料だが、SaaS 版はリクエスト数や同時接続数に応じて課金される。
- 自前実行でも仮想マシンの利用料・ネットワーク帯域が発生。テスト対象側 (Cloud Run 等) の利用量も課金対象。
- 工数 (シナリオ設計・分析) も見積もりに含める。

## 秒間 200 リクエスト × 10 秒の再現方法
- `k6` OSS 版を利用し、仮想ユーザー 200・実行時間 10 秒で設定。
- Docker 実行例:
  ```bash
  docker run --rm -i loadimpact/k6 run - <<'SCRIPT'
  import http from 'k6/http';

  export const options = {
    vus: 200,
    duration: '10s',
    thresholds: {
      http_req_failed: ['rate<0.01'],
      http_req_duration: ['p(95)<500'],
    },
  };

  export default function () {
    http.get('https://your-endpoint.example.com/api');
  }
  SCRIPT
  ```
- `hey -z 10s -q 200 https://...` などの軽量ツールでも可。実測の RPS を確認し、必要に応じて VUS や並列数を調整。
- Cloud Run・Supabase・Upstash のメトリクスを監視し、無料枠の消費やレート制限兆候があれば即停止できるようにする。
