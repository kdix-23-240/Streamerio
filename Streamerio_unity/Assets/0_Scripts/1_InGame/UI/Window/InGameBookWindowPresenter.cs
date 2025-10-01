using System.Threading;
using Common.UI.Display.Window.Book;
using Common.UI.Display.Window.Panel;
using Cysharp.Threading.Tasks;

namespace InGame.UI.Window
{
    /// <summary>
    /// インゲーム用の「本型ウィンドウ」プレゼンター。
    /// - BookWindowPresenterBase を継承
    /// - Show/ShowAsync 時に必ず「遊び方（HowToPlay）」チャプターを開く
    /// </summary>
    public class InGameBookWindowPresenter : BookWindowPresenterBase
    {
        /// <summary>
        /// 非同期表示処理。
        /// - ウィンドウを表示した後、自動的に HowToPlay チャプターを開き、
        ///   チャプターが閉じられるまで待機する
        /// </summary>
        public override async UniTask ShowAsync(CancellationToken ct)
        {
            await base.ShowAsync(ct);
            await ChapterManager.Instance.OpenAndWaitCloseAsync<HowToPlayChapterPanelPresenter>(ct);
        }
        
        /// <summary>
        /// 即時表示処理。
        /// - ウィンドウを表示した後、HowToPlay チャプターを開く
        /// - 非同期版と違い「閉じるまで待機」は行わない
        /// </summary>
        public override void Show()
        {
            base.Show();
            ChapterManager.Instance.OpenDisplay<HowToPlayChapterPanelPresenter>();
        }
    }
}