using Common.Scene;
using Common.UI.Display.Overlay;
using Cysharp.Threading.Tasks;
using R3;

namespace InGame.UI.Display.Overlay
{
    public class ClearOverlayPresenter: OverlayPresenterBase<ClearOverlayView>
    {
        protected override void SetEvent()
        {
            base.SetEvent();
            
            OnClickAsObservable
                .SubscribeAwait( async (_, ct )=>
                {
                    await HideAsync(ct);
                    SceneManager.Instance.LoadSceneAsync(SceneType.Title).Forget();
                }).RegisterTo(destroyCancellationToken);
                
        }
    }
}