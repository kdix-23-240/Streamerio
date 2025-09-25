using Common;
using Common.UI.Display.Overlay;
using Common.UI.Loading;
using Cysharp.Threading.Tasks;
using OutGame.GameOver.Overlay;
using UnityEngine;

namespace OutGame.GameOver
{
    public class GameOverManager:SingletonBase<GameOverManager>
    {
        private async void Start()
        {
            OverlayManager.Instance.Initialize();

            await LoadingScreenPresenter.Instance.HideAsync();

            await OverlayManager.Instance.OpenAndWaitCloseAsync<GameOverOverlayPresenter>(destroyCancellationToken);
        }
    }
}