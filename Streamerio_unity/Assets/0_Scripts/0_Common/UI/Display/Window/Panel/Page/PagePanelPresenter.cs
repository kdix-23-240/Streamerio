using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Common.UI.Display.Window.Panel
{
    /// <summary>
    /// ページのつなぎ役
    /// </summary>
    [RequireComponent(typeof(PagePanelView))]
    public class PagePanelPresenter: DisplayPresenterBase<PagePanelView>
    {
        public override async UniTask ShowAsync(CancellationToken ct)
        {
            await View.ShowAsync(ct);
        }

        public override void Show()
        {
            View.Show();
        }

        public override async UniTask HideAsync(CancellationToken ct)
        {
            await View.HideAsync(ct);
        }

        public override void Hide()
        {
            View.Hide();
        }
    }
}