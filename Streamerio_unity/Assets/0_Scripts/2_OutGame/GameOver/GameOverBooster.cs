using Common;
using Common.UI.Display;
using Common.UI.Display.Overlay;
using Common.UI.Loading;
using OutGame.UI.Display.Overlay;

namespace OutGame.GameOver
{
    /// <summary>
    /// ゲームオーバー時の UI 起動処理を担うクラス。
    /// - Overlay 系 UI の初期化
    /// - ローディング画面の非表示
    /// - GameOverOverlay を開き、閉じられるまで待機
    /// </summary>
    public class GameOverBooster : SingletonBase<GameOverBooster>
    {
        /// <summary>
        /// ゲームオーバー演出の開始処理。
        /// Unity の Start で非同期フローを実行する。
        /// 1) UI の初期化
        /// 2) ローディング画面を閉じる
        /// 3) GameOverOverlay を開いて、閉じられるまで待機
        /// </summary>
        private async void Start()
        {
            // 1) UI の初期化
            DisplayBooster.Instance.Boost();

            // 2) ローディング画面を非表示
            await LoadingScreenPresenter.Instance.HideAsync();

            // 3) GameOverOverlay を開いて、ユーザーが閉じるまで待機
            await OverlayManager.Instance.OpenAndWaitCloseAsync<GameOverOverlayPresenter>(destroyCancellationToken);
        }
    }
}