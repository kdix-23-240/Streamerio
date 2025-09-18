using Common;
using Common.UI.Display.Window;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace OutGame.Title
{
    public class TitleManager: SingletonBase<TitleManager>
    {
        [SerializeField]
        private CommonWindowPresenter _window;
        
        private async void Start()
        {
            _window.Initialize();
            _window.Hide();

            await UniTask.WaitForSeconds(3);
            _window.ShowAsync(destroyCancellationToken).Forget();
        }
    }
}