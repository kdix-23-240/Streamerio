using System.Threading;
using Common.UI.Display.Window.Book;
using Common.UI.Display.Window.Panel;
using Cysharp.Threading.Tasks;

namespace OutGame.UI.Display.Window
{
    /// <summary>
    /// アウトゲーム用の「本型ウィンドウ」プレゼンター。
    /// - BookWindowPresenterBase を継承し、最初に表示するチャプターをメニューチャプターに固定
    /// - Show/ShowAsync のタイミングで ChapterManager を通じて MenuChapterPanel を開く
    /// </summary>
    public class OutGameBookWindowPresenter : BookWindowPresenterBase
    {
        /// <summary>
        /// 非同期表示処理。
        /// - ウィンドウを表示した後、自動的にメニューチャプターを開く
        /// - チャプターが閉じられるまで待機する
        /// </summary>
        public override async UniTask ShowAsync(CancellationToken ct)
        {
            await base.ShowAsync(ct);
            await ChapterManager.Instance.OpenAndWaitCloseAsync<MenuChapterPanelPresenter>(ct);
        }
        
        /// <summary>
        /// 即時表示処理。
        /// - ウィンドウを表示した後、メニューチャプターを開く
        /// - 非同期版と異なり「閉じるまで待機」は行わない
        /// </summary>
        public override void Show()
        {
            base.Show();
            ChapterManager.Instance.OpenDisplay<MenuChapterPanelPresenter>();
        }
    }
}