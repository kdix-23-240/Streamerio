using _0_Scripts._2_OutGame.UI.Display.Screen;
using Common;
using Common.UI.Display.Window;
using Cysharp.Threading.Tasks;
using OutGame.UI.Display.Window;
using UnityEngine;

namespace OutGame.Title
{
    public class TitleManager: SingletonBase<TitleManager>
    {
        [SerializeField]
        private TitleScreenPresenter _screen;
        [SerializeField]
        private TitleWindowPresenter _window;
        
        private void Start()
        {
            _screen.Initialize();
            _screen.Show();
            
            _window.Initialize();
            _window.Hide();
        }
        
        /// <summary>
        /// ウィンドウを開く
        /// </summary>
        public async UniTask OpenTitleWindowAsync()
        {
            await _window.ShowAsync(destroyCancellationToken);
        }
        
        /// <summary>
        /// スクリーンを表示する
        /// </summary>
        public void ShowScreen()
        {
            _screen.Show();
        }
    }
}