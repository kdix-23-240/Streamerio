using Cysharp.Threading.Tasks;
using System.Threading;

namespace Common.UI.Display.Overlay
{
    public class OverlayPresenterBase<TView>: DisplayPresenterBase<TView>
        where TView: OverlayViewBase
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