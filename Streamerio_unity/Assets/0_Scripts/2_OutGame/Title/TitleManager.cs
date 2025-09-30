using System.Threading;
using Common;
using Common.Audio;
using Common.UI.Display.Window;
using Common.UI.Loading;
using Cysharp.Threading.Tasks;
using OutGame.UI.Display.Screen;
using OutGame.UI.Display.Window;
using UnityEngine;

namespace OutGame.Title
{
    public class TitleManager: SingletonBase<TitleManager>
    {
        [SerializeField]
        private TitleScreenPresenter _screen;
        [SerializeField]
        private OutGameBookWindowPresenter _window;
        
        private void Start()
        {
            AudioManager.Instance.PlayAsync(BGMType.kuraituuro, destroyCancellationToken).Forget();
            
            _screen.Initialize();
            _screen.Show();
            
            _window.Initialize();
            _window.Hide();
            
            LoadingScreenPresenter.Instance.HideAsync().Forget();
        }
        
        /// <summary>
        /// ウィンドウを開く
        /// </summary>
        public async UniTask OpenTitleWindowAsync(CancellationToken ct)
        {
            await _window.ShowAsync(ct);
            await _screen.ShowAsync(ct);
        }
        
        public async UniTask LoadTitleAsync()
        {
            await LoadingScreenPresenter.Instance.TitleToLoadingAsync();
            AudioManager.Instance.StopBGM();
        }
    }
}