using System.Threading;
using Common.UI.Display.Window.Book;
using Common.UI.Display.Window.Panel;
using Cysharp.Threading.Tasks;
using OutGame.Title;

namespace OutGame.UI.Display.Window
{
    /// <summary>
    /// アウトゲーム用の「本型ウィンドウ」プレゼンター。
    /// - BookWindowPresenterBase を継承し、メニュー章（MenuChapter）を入り口として開く
    /// - 表示/非表示のタイミングで ChapterManager を介してページを制御
    /// - 非表示完了後は TitleManager に画面復帰（タイトルスクリーン再表示）を依頼
    /// </summary>
    public class OutGameBookWindowPresenter : BookWindowPresenterBase
    {
        /// <summary>
        /// 非同期表示処理。
        /// 1) 基底のウィンドウ表示（背景や本体アニメ等）
        /// 2) メニュー章を「開いて閉じるまで待つ」
        ///    - メニューを閉じると BookWindowPresenterBase 側の IsBusy 監視で
        ///      自動的にウィンドウも閉じるフロー（設計に応じて）に接続可能
        /// </summary>
        public override async UniTask ShowAsync(CancellationToken ct)
        {
            await base.ShowAsync(ct);
            await ChapterManager.Instance.OpenAndWaitCloseAsync<MenuChapterPanelPresenter>(ct);
        }
        
        /// <summary>
        /// 即時表示処理。
        /// 1) 基底のウィンドウ表示（即時）
        /// 2) メニュー章を「開く」（閉じ待ちはしない）
        /// </summary>
        public override void Show()
        {
            base.Show();
            ChapterManager.Instance.OpenDisplay<MenuChapterPanelPresenter>();
        }

        /// <summary>
        /// 非同期非表示処理。
        /// - 基底の非表示（背景/本体のクローズ）を終えたら
        ///   タイトル側へ「タイトルスクリーン再表示」を依頼。
        ///   ※ウィンドウを閉じた後にタイトル画面へ戻す役割を明確化。
        /// </summary>
        public override async UniTask HideAsync(CancellationToken ct)
        { 
            await base.HideAsync(ct);
            TitleManager.Instance.ShowScreen(); // タイトル画面を再表示
        }
        
        /// <summary>
        /// 即時非表示処理。
        /// - 即時に閉じた後、タイトル側へ再表示を依頼
        /// </summary>
        public override void Hide()
        {
            base.Hide();
            TitleManager.Instance.ShowScreen(); // タイトル画面を再表示
        }
    }
}