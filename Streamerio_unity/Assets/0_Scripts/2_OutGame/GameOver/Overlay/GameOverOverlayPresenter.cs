using Alchemy.Inspector;
using Common.Save;
using Common.Scene;
using Common.UI.Display.Overlay;
using Common.UI.Loading;
using Cysharp.Threading.Tasks;
using R3;
using UnityEngine;

namespace OutGame.GameOver.Overlay
{
    /// <summary>
    /// ゲームオーバー時のオーバーレイ Presenter。
    /// - Retry / Title ボタンのイベント処理を担当
    /// - ボタンクリック時にロード画面を表示し、シーン遷移を実行
    /// - リトライ時は SaveManager にフラグを設定
    /// </summary>
    [RequireComponent(typeof(GameOverOverlayView))]
    public class GameOverOverlayPresenter : OverlayPresenterBase
    {
        [SerializeField, ReadOnly]
        private GameOverOverlayView _view;

#if UNITY_EDITOR
        /// <summary>
        /// エディタ上でコンポーネント参照を自動補完
        /// </summary>
        protected override void OnValidate()
        {
            base.OnValidate();
            _view ??= GetComponent<GameOverOverlayView>();
        }
#endif

        /// <summary>
        /// 初期化処理。
        /// - View の初期化
        /// - 基底クラスの初期化処理を呼び出し
        /// </summary>
        public override void Initialize()
        {
            _view.Initialize();
            base.Initialize();
        }

        /// <summary>
        /// イベント購読のセットアップ。
        /// - Retry ボタン → ローディング画面を表示 → Retry フラグ設定 → GameScene 再ロード
        /// - Title ボタン → ローディング画面を表示 → Title シーンへ遷移
        /// </summary>
        protected override void Bind()
        {
            base.Bind();

            // リトライボタンクリック
            _view.RetryButton.OnClickAsObservable
                .SubscribeAwait(async (_, ct) =>
                {
                    await LoadingScreenPresenter.Instance.ShowAsync();
                    SaveManager.Instance.IsRetry = true;
                    SceneManager.Instance.LoadSceneAsync(SceneType.GameScene).Forget();
                })
                .RegisterTo(destroyCancellationToken);
            
            // タイトルボタンクリック
            _view.TitleButton.OnClickAsObservable
                .SubscribeAwait(async (_, ct) =>
                {
                    await LoadingScreenPresenter.Instance.ShowAsync();
                    SceneManager.Instance.LoadSceneAsync(SceneType.Title).Forget();
                })
                .RegisterTo(destroyCancellationToken);
        }
    }
}