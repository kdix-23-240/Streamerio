using System.Threading;
using Common.UI.Display.Window.Book;
using Common.UI.Display.Window.Panel;
using Cysharp.Threading.Tasks;
using R3;

namespace InGame.UI.Window
{
    public class InGameBookWindowPresenter: BookWindowPresenterBase
    {
        public override async UniTask ShowAsync(CancellationToken ct)
        {
            await base.ShowAsync(ct);
            await ChapterManager.Instance.OpenAndWaitCloseAsync<HowToPlayChapterPanelPresenter>(ct);
        }
        
        public override void Show()
        {
            base.Show();
            ChapterManager.Instance.OpenDisplay<HowToPlayChapterPanelPresenter>();
        }
    }
}