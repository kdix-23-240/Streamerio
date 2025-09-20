using Common.Scene;
using Common.UI.Display.Overlay;
using Cysharp.Threading.Tasks;

namespace InGame.UI.Display.Overlay.GameOver
{
    public class GameOverOverlayPresenter: OverlayPresenterBase<GameOverOverlayView>
    {
        protected override void Bind()
        {
            base.Bind();
            View.RetryButton.SetClickEvent(async () =>
            {
                await HideAsync(destroyCancellationToken);
                SceneManager.Instance.ReloadSceneAsync().Forget();
            });
            
            View.TitleButton.SetClickEvent(async () =>
            {
                await HideAsync(destroyCancellationToken);
                SceneManager.Instance.LoadSceneAsync(SceneType.Title).Forget();
            });
        }
    }
}