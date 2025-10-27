using Common;
using Common.Scene;
using Common.UI.Display.Overlay;
using Common.UI.Loading;
using Cysharp.Threading.Tasks;
using R3;

namespace OutGame.UI.GameOver
{
    public interface IGameOverOverlay: IOverlay, IAttachable<GameOverOverlayContext>
    {
        
    }
    
    /// <summary>
    /// ゲームオーバー時のオーバーレイ Presenter。
    /// - Retry / Title ボタンのイベント処理を担当
    /// - ボタンクリック時にロード画面を表示し、シーン遷移を実行
    /// - リトライ時は SaveManager にフラグを設定
    /// </summary>
    public class GameOverOverlayPresenter : OverlayPresenterBase<IGameOverOverlayView, GameOverOverlayContext>, IGameOverOverlay
    {
        private ISceneManager _sceneManager;
        private ILoadingScreen _loadingScreen;

        protected override void AttachContext(GameOverOverlayContext context)
        {
            base.AttachContext(context);

            _sceneManager = context.SceneManager;
            _loadingScreen = context.LoadingScreen;
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
            View.RetryButton.OnClickAsObservable
                .SubscribeAwait(async (_, ct) =>
                {
                    await _loadingScreen.ShowAsync(ct);
                    _sceneManager.ReloadSceneAsync();
                })
                .RegisterTo(GetCt());
            
            // タイトルボタンクリック
            View.TitleButton.OnClickAsObservable
                .SubscribeAwait(async (_, ct) =>
                {
                    await _loadingScreen.ShowAsync(ct);
                    _sceneManager.LoadSceneAsync(SceneType.Title).Forget();
                })
                .RegisterTo(GetCt());
        }
    }

    public class GameOverOverlayContext : CommonOverlayContext<IGameOverOverlayView>
    {
        public ISceneManager SceneManager;
        public ILoadingScreen LoadingScreen;
    }
}