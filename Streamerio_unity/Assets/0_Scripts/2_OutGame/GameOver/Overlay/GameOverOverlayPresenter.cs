using Common.Save;
using Common.Scene;
using Common.UI.Display.Overlay;
using Common.UI.Loading;
using Cysharp.Threading.Tasks;

namespace OutGame.GameOver.Overlay
{
    public class GameOverOverlayPresenter: OverlayPresenterBase<GameOverOverlayView>
    {
        protected override void Bind()
        {
            base.Bind();
            View.RetryButton.SetClickEvent(async () =>
            {
                await LoadingScreenPresenter.Instance.ShowAsync();
                SaveManager.Instance.IsRetry = true;
                SceneManager.Instance.LoadSceneAsync(SceneType.GameScene).Forget();
            });
            
            View.TitleButton.SetClickEvent(async () =>
            {
                await LoadingScreenPresenter.Instance.ShowAsync();
                SceneManager.Instance.LoadSceneAsync(SceneType.Title).Forget();
            });
        }
    }
}