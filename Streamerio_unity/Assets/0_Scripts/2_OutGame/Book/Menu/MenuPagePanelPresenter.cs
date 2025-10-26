using Common;
using Common.UI.Display.Window.Book.Chapter;
using Common.UI.Display.Window.Book.Page;
using R3;

namespace OutGame.Book.Menu
{
    public interface IMenuPagePanel : IPagePanel, IAttachable<MenuPagePanelContext>
    {
    }
    
    /// <summary>
    /// メニューページのプレゼンター。
    /// - MenuPagePanelView と連携し、ボタン操作を購読する
    /// - 「スタート/遊び方/オプション/クレジット」各ボタンのイベントをバインド
    /// - ボタンクリックに応じて、シーン遷移や UI 遷移を制御する
    /// </summary>
    public class MenuPagePanelPresenter : PagePanelPresenterBase<IMenuPagePanelView, MenuPagePanelContext>, IMenuPagePanel
    {
        private IBookWindowModel _bookWindowModel;

        protected override void AttachContext(MenuPagePanelContext context)
        {
            base.AttachContext(context);
            _bookWindowModel = context.BookWindowModel;
        }

        /// <summary>
        /// ボタンイベントのバインド処理。
        /// - 各種ボタンクリックに対応する遷移/処理を登録
        /// </summary>
        protected override void Bind()
        {
            base.Bind();

            // ゲーム開始ボタン
            View.StartButton.OnClickAsObservable
                .Subscribe(_ =>
                {
                    Debug.Log("ゲームスタート");
                }).RegisterTo(GetCt());
            
            // 遊び方ボタン
            View.HowToPlayButton.OnClickAsObservable
                .Subscribe(_ =>
                {
                    _bookWindowModel.PushChapter(ChapterType.HowToPlay);
                }).RegisterTo(GetCt());
            
            // オプションボタン
            View.OptionButton.OnClickAsObservable
                .Subscribe(_ =>
                {
                    _bookWindowModel.PushChapter(ChapterType.Option);
                }).RegisterTo(GetCt());
            
            // クレジットボタン
            View.CreditButton.OnClickAsObservable
                .Subscribe(_ =>
                {
                    _bookWindowModel.PushChapter(ChapterType.Credit);
                }).RegisterTo(GetCt());
        }
    }
    
    public class MenuPagePanelContext : PagePanelContext<IMenuPagePanelView>
    {
        public IBookWindowModel BookWindowModel;
    }
}