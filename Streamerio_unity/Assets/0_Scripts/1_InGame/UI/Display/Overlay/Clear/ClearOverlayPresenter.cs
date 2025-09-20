using Common.Scene;
using Common.UI.Display.Overlay;
using Common.UI.Loading;
using Cysharp.Threading.Tasks;
using R3;

namespace InGame.UI.Display.Overlay
{
    public class ClearOverlayPresenter: OverlayPresenterBase<ClearOverlayView>
    {
        protected override void SetEvent()
        {
            base.SetEvent();
            
            View.Background.OnClickAsObservable
                .Subscribe( async _ =>
                {
                    Debug.Log("ClearOverlayPresenter: Clicked");
                    await LoadingScreenPresenter.Instance.ShowAsync();
                    SceneManager.Instance.LoadSceneAsync(SceneType.Title).Forget();
                }).RegisterTo(destroyCancellationToken);
                
        }
    }
}