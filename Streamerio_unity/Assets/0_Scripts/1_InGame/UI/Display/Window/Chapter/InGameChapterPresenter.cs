using Common.UI.Display.Window.Panel;

namespace InGame.UI.Display.Window.Chapter
{
    /// <summary>
    /// インゲームのチャプター
    /// </summary>
    public class InGameChapterPresenter: ChapterPanelPresenterBase
    {
        protected override void AllCloseEvent()
        {
            InGameManager.Instance.OpenQRCodeDialog();
        }
    }
}