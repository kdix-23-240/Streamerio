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
    /// 章のパネルのプレゼンター基底クラス。
    /// - ChapterPanelView と連携してページを表示/非表示する
    /// - ボタン入力（次/前/閉じる）を購読し、遷移アニメーションを制御する
    /// - IDisplay を実装し、外部から開閉を統一的に扱える
    /// - BookWindowAnimation を用いて「本をめくる」演出を付与
    /// </summary>
    [RequireComponent(typeof(ChapterPanelView))]
    public abstract class ChapterPanelPresenterBase : UIBehaviour, IDisplay
    {
        [SerializeField, ReadOnly]
        protected ChapterPanelView _view;

        private bool _isShow;
        /// <summary>現在表示中かどうか</summary>
        public bool IsShow => _isShow;
        
        private int _currentIndex; // 現在のページ番号
        private BookWindowAnimation _bookWindowAnimation; // ページめくり演出

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();
            _view ??= GetComponent<ChapterPanelView>();
        }
#endif

        /// <summary>
        /// 今回は使用しない
        /// </summary>
        public void Initialize() { }

        /// <summary>
        /// 初期化処理。
        /// - BookWindowAnimation を受け取り保持
        /// - ページ番号をリセット
        /// - View の初期化
        /// - ボタンイベントの購読をセット
        /// </summary>
        public void Initialize(BookWindowAnimation bookWindowAnimation)
        {
            _bookWindowAnimation = bookWindowAnimation;
            _currentIndex = 0;

            _view.Initialize();
            Bind();
        }
        
        /// <summary>
        /// ボタンイベントを購読し、ページ遷移や閉じる処理を登録。
        /// </summary>
        private void Bind()
        {
            // 次のページ
            _view.NextButton.OnClickAsObservable
                .Subscribe(_ => { OpenNextPage(destroyCancellationToken).Forget(); })
                .RegisterTo(destroyCancellationToken);
            
            // 前のページ
            _view.BackButton.OnClickAsObservable
                .Subscribe(_ => { OpenPrePage(destroyCancellationToken).Forget(); })
                .RegisterTo(destroyCancellationToken);
            
            // 閉じる
            _view.CloseButton.OnClickAsObservable
                .SubscribeAwait(async (_, ct) =>
                {
                    await ChapterManager.Instance.CloseTopAsync(ct);
                })
                .RegisterTo(destroyCancellationToken);
        }
        
        /// <summary>
        /// 表示（アニメーション付き）
        /// - ページ番号をリセットして最初のページを表示
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
        /// 即時表示
        /// - ページ番号をリセットして最初のページを表示
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
        /// 現在表示中のページをアニメーションで非表示
        /// </summary>
        public async UniTask HideAsync(CancellationToken ct)
        {
            _view.SetInteractable(false);
            
            await _view.HidePageAsync(_currentIndex, ct);
            
            _view.Hide();
            _isShow = false;
        }

        /// <summary>
        /// 現在表示中のページを即時非表示
        /// </summary>
        public void Hide()
        {
            _view.SetInteractable(false);
            
            _view.HidePage(_currentIndex);
            
            _view.Hide();
            _isShow = false;
        }
        
        /// <summary>
        /// 次のページを開く。
        /// - 現在のページを閉じる
        /// - ページめくり演出（右めくり）
        /// - 次のページを表示
        /// </summary>
        private async UniTask OpenNextPage(CancellationToken ct)
        {
            _view.HidePageAsync(_currentIndex, ct).Forget();
            await _bookWindowAnimation.PlayTurnRightAsync(ct);
            await ShowPageAsync(_currentIndex + 1, ct);
        }

        /// <summary>
        /// 前のページを開く。
        /// - 現在のページを閉じる
        /// - ページめくり演出（左めくり）
        /// - 前のページを表示
        /// </summary>
        private async UniTask OpenPrePage(CancellationToken ct)
        {
            _view.HidePageAsync(_currentIndex, ct).Forget();
            await _bookWindowAnimation.PlayTurnLeftAsync(ct);
            await ShowPageAsync(_currentIndex - 1, ct);
        }

        /// <summary>
        /// 指定ページを表示。
        /// - Index を範囲内にClampして安全に遷移
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
