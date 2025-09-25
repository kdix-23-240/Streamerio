using Common.Save;
using Common.Scene;
using Common.UI.Display.Overlay;
using Common.UI.Loading;
using Cysharp.Threading.Tasks;
using R3;

namespace OutGame.GameOver.Overlay
{
    public class GameOverOverlayPresenter: OverlayPresenterBase<GameOverOverlayView>
    {
        protected override void Bind()
        {
            base.Bind();
            View.RetryButton.OnClickAsObservable
                .SubscribeAwait(async (_, ct) =>
                {
                    await LoadingScreenPresenter.Instance.ShowAsync();
                    SaveManager.Instance.IsRetry = true;
                    SceneManager.Instance.LoadSceneAsync(SceneType.GameScene).Forget();
                }).RegisterTo(destroyCancellationToken);
            
            View.TitleButton.OnClickAsObservable
                .SubscribeAwait(async (_, ct) =>
                {
                    await LoadingScreenPresenter.Instance.ShowAsync();
                    SceneManager.Instance.LoadSceneAsync(SceneType.Title).Forget();
                }).RegisterTo(destroyCancellationToken);
        }
    }
}