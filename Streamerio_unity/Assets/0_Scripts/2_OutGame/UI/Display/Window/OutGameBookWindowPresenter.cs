using System.Threading;
using Common.UI.Display.Window.Book;
using Common.UI.Display.Window.Panel;
using Cysharp.Threading.Tasks;

namespace OutGame.UI.Display.Window
{
    public class OutGameBookWindowPresenter: BookWindowPresenterBase
    {
        public override async UniTask ShowAsync(CancellationToken ct)
        {
            await base.ShowAsync(ct);
            await ChapterManager.Instance.OpenAndWaitCloseAsync<MenuChapterPanelPresenter>(ct);
        }
        
        public override void Show()
        {
            base.Show();
            ChapterManager.Instance.OpenDisplay<MenuChapterPanelPresenter>();
        }
    }
}