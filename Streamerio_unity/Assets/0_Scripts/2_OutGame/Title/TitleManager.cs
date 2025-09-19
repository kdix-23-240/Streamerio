using System.Threading;
using Common;
using Common.UI.Display.Window;
using Common.UI.Loading;
using Cysharp.Threading.Tasks;
using OutGame.UI.Display.Screen;
using UnityEngine;

namespace OutGame.Title
{
    public class TitleManager: SingletonBase<TitleManager>
    {
        [SerializeField]
        private TitleScreenPresenter _screen;
        [SerializeField]
        private WindowPresenter _window;
        
        private void Start()
        {
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
        }
        
        /// <summary>
        ///  タイトルスクリーンを表示する
        /// </summary>
        public async UniTask ShowTitleAsync(CancellationToken ct)
        {
            await _window.HideAsync(ct);
            await _screen.ShowAsync(ct);
        }
        
        public async UniTask LoadTitleAsync()
        {
            await _window.HideAsync(destroyCancellationToken);
            await LoadingScreenPresenter.Instance.TitleToLoadingAsync();
        }
    }
}