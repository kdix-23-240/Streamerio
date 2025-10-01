using System;
using Common;
using Common.UI.Display;
using Common.UI.Display.Window;
using Common.UI.Loading;
using OutGame.UI.Display.Window;
using UnityEngine;

namespace OutGame.GameClear
{
    public class GameClearBooster: MonoBehaviour
    {
        private async void Start()
        {
            DisplayBooster.Instance.Boost();
            
            // 2) ローディング画面を非表示
            await LoadingScreenPresenter.Instance.HideAsync();

            // 3) GameOverOverlay を開いて、ユーザーが閉じるまで待機
            await WindowManager.Instance.OpenAndWaitCloseAsync<GameClearWindowPresenter>(destroyCancellationToken);
        }
    }
}