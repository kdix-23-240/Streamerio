using Common.UI.Display.Window.Panel;
using Cysharp.Threading.Tasks;
using OutGame.Title;

namespace OutGame.UI.Display.Window.Panel.Chapter
{
    public class TitleChapterPresenter: ChapterPanelPresenterBase
    {
        protected override void AllCloseEvent()
        {
            TitleManager.Instance.ShowTitleAsync(destroyCancellationToken).Forget();
        }
    }
}