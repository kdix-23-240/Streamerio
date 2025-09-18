using System.Threading;
using Common.UI.Display.Window;
using Cysharp.Threading.Tasks;
using OutGame.Title;
using UnityEngine;

namespace OutGame.UI.Display.Window
{
    [RequireComponent(typeof(TitleWindowView))]
    public class TitleWindowPresenter: WindowPresenterBase<TitleWindowView>
    {
        public override async UniTask HideAsync(CancellationToken ct)
        {
            await base.HideAsync(ct);
            TitleManager.Instance.ShowScreen();
        }
    }
}