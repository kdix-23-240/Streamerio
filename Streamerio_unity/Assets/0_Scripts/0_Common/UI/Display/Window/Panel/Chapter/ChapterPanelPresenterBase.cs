using System.Threading;
using Alchemy.Inspector;
using Common.UI.Display.Window.Book;
using Cysharp.Threading.Tasks;
using R3;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Common.UI.Display.Window.Panel
{
    /// <summary>
    /// 章（Chapter）パネルの Presenter 基底クラス。
    /// 役割:
    /// - ChapterPanelView と連携してページの表示/非表示を制御
    /// - 次/前/閉じるボタンの入力を購読し、ページ遷移と演出（BookWindowAnimation）を行う
    /// - IDisplay を実装して外部から統一的な開閉操作を受け付ける
    ///
    /// 注意:
    /// - 遷移中は View を非インタラクティブにし、完了後に戻す
    /// - ページ番号は常に Clamp して範囲外アクセスを防止
    /// </summary>
    [RequireComponent(typeof(ChapterPanelView))]
    public abstract class ChapterPanelPresenterBase : UIBehaviour, IDisplay
    {
        [SerializeField, ReadOnly]
        protected ChapterPanelView _view;

        private bool _isShow;
        /// <summary>現在表示中かどうか</summary>
        public bool IsShow => _isShow;
        
        private int _currentIndex;                       // 現在のページ番号（0 始まり）
        private BookWindowAnimation _bookWindowAnimation; // ページめくり演出

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();
            _view ??= GetComponent<ChapterPanelView>();
        }
#endif

        /// <summary>
        /// IDisplay の規約上のメソッドだが、本系では未使用。
        /// </summary>
        public void Initialize() { }

        /// <summary>
        /// 初期化。
        /// 手順:
        /// 1) BookWindowAnimation を受け取って保持
        /// 2) 現在ページを 0 にリセット
        /// 3) View を初期化
        /// 4) ボタン入力を購読
        /// </summary>
        public void Initialize(BookWindowAnimation bookWindowAnimation)
        {
            _bookWindowAnimation = bookWindowAnimation;
            _currentIndex = 0;

            _view.Initialize();
            Bind();
        }
        
        /// <summary>
        /// ボタン入力を購読し、ページ遷移/クローズを登録。
        /// - 次へ: OpenNextPage
        /// - 前へ: OpenPrePage
        /// - 閉じる: ChapterManager.CloseTopAsync
        /// </summary>
        private void Bind()
        {
            _view.NextButton.OnClickAsObservable
                .SubscribeAwait(async (_, ct) => { await OpenNextPage(ct); })
                .RegisterTo(destroyCancellationToken);
            
            _view.BackButton.OnClickAsObservable
                .SubscribeAwait(async (_, ct) => { await OpenPrePage(ct); })
                .RegisterTo(destroyCancellationToken);
            
            _view.CloseButton.OnClickAsObservable
                .SubscribeAwait(async (_, ct) => { await ChapterManager.Instance.CloseTopAsync(ct); })
                .RegisterTo(destroyCancellationToken);
        }
        
        /// <summary>
        /// 表示（アニメーションあり）。
        /// - 最初のページから表示を開始
        /// - 遷移中は操作禁止
        /// </summary>
        public async UniTask ShowAsync(CancellationToken ct)
        {
            _view.SetInteractable(true);
            _isShow = true;
            _view.Show();
            
            _currentIndex = 0;
            await ShowPageAsync(_currentIndex, ct);
        }

        /// <summary>
        /// 即時表示。
        /// - 最初のページから表示を開始
        /// </summary>
        public void Show()
        {
            _view.SetInteractable(true);
            _isShow = true;
            _view.Show();
            
            _currentIndex = 0;
            _view.ShowPage(_currentIndex);
        }
        
        /// <summary>
        /// 非表示（アニメーションあり）。
        /// - 現在ページを閉じてから全体を隠す
        /// - 遷移中は操作禁止
        /// </summary>
        public async UniTask HideAsync(CancellationToken ct)
        {
            _view.SetInteractable(false);
            await _view.HidePageAsync(_currentIndex, ct);
            _view.Hide();
            _isShow = false;
        }

        /// <summary>
        /// 即時非表示。
        /// - 現在ページを閉じてから全体を隠す
        /// </summary>
        public void Hide()
        {
            _view.SetInteractable(false);
            _view.HidePage(_currentIndex);
            _view.Hide();
            _isShow = false;
        }
        
        /// <summary>
        /// 次のページへ。
        /// 流れ:
        /// 1) 現ページを非表示（非同期）
        /// 2) 右めくり演出
        /// 3) 次ページを表示
        /// 4) 操作再開
        /// </summary>
        private async UniTask OpenNextPage(CancellationToken ct)
        {
            _view.SetInteractable(false);
            _view.HidePageAsync(_currentIndex, ct).Forget();
            await _bookWindowAnimation.PlayTurnRightAsync(ct);
            await ShowPageAsync(_currentIndex + 1, ct);
            _view.SetInteractable(true);
        }

        /// <summary>
        /// 前のページへ。
        /// 流れ:
        /// 1) 現ページを非表示（非同期）
        /// 2) 左めくり演出
        /// 3) 前ページを表示
        /// 4) 操作再開
        /// </summary>
        private async UniTask OpenPrePage(CancellationToken ct)
        {
            _view.SetInteractable(false);
            _view.HidePageAsync(_currentIndex, ct).Forget();
            await _bookWindowAnimation.PlayTurnLeftAsync(ct);
            await ShowPageAsync(_currentIndex - 1, ct);
            _view.SetInteractable(true);
        }

        /// <summary>
        /// 指定ページを表示する共通処理。
        /// - nextIndex を 0..Last で Clamp
        /// - ページ位置に応じて「前へ/次へ」ボタンの表示を切り替え
        /// - 目的ページをアニメーションで表示
        /// </summary>
        private async UniTask ShowPageAsync(int nextIndex, CancellationToken ct)
        {
            _currentIndex = Mathf.Clamp(nextIndex, 0, _view.LastPageIndex);

            _view.BackButton.SetActive(_currentIndex != 0);
            _view.NextButton.SetActive(_currentIndex != _view.LastPageIndex);
            
            await _view.ShowPageAsync(_currentIndex, ct);
        }
    }
}
