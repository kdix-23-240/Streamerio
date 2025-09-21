using Common;
using Common.UI.Loading;
using Cysharp.Threading.Tasks;
using OutGame.GameOver.Overlay;
using UnityEngine;

namespace OutGame.GameOver
{
    public class GameOverManager:SingletonBase<GameOverManager>
    {
        [SerializeField] private GameOverOverlayPresenter _overlay;
        
        private async void Start()
        {
            _overlay.Initialize();
            _overlay.Hide();

            await LoadingScreenPresenter.Instance.HideAsync();
            
            _overlay.ShowAsync(destroyCancellationToken).Forget();
        }
    }
}