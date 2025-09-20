using System.Threading;
using Common.UI.Display.Window.Animation;
using Cysharp.Threading.Tasks;
using OutGame.Title;
using R3;
using UnityEngine;

namespace Common.UI.Display.Window.Panel
{
    /// <summary>
    /// 章のパネルの繋ぎ役
    /// </summary>
    [RequireComponent(typeof(ChapterPanelView))]
    public class ChapterPanelPresenter: DisplayPresenterBase<ChapterPanelView>
    {
        private ReactiveProperty<int> _currentIndexProp;
        private int _currentIndex => _currentIndexProp.Value;

        private BookWindowAnimation _bookWindowAnimation;

        /// <summary>
        /// 初期化
        /// </summary>
        /// <param name="bookWindowAnimation"></param>
        public void Initialize(BookWindowAnimation bookWindowAnimation)
        {
            _currentIndexProp = new ReactiveProperty<int>();

            _bookWindowAnimation = bookWindowAnimation;

            base.Initialize();
        }

        /// <summary>
        /// イベント設定
        /// </summary>
        protected override void SetEvent()
        {
            base.SetEvent();
            
            View.NextButton.SetClickEvent(()=> OpenNextPage(destroyCancellationToken).Forget());
            View.BackButton.SetClickEvent(()=> OpenPrePage(destroyCancellationToken).Forget());
            View.CloseButton.SetClickEvent(async () =>
            {
                var  isAllClose = await ChapterManager.Instance.CloseChapterAsync(destroyCancellationToken);
                if (isAllClose)
                {
                    TitleManager.Instance.ShowTitleAsync(destroyCancellationToken).Forget();
                }
            });
        }
        
        protected override void Bind()
        {
            _currentIndexProp
                .Subscribe(_ =>
                {
                    View.BackButton.gameObject.SetActive(_currentIndex > 0);
                    View.NextButton.gameObject.SetActive(_currentIndex < View.LastPageIndex);
                }).RegisterTo(destroyCancellationToken);
        }
        
        public override async UniTask ShowAsync(CancellationToken ct)
        {
            View.SetInteractable(true);
            View.Show();
            
            _currentIndexProp.Value = 0;
            await ShowPageAsync(_currentIndex, ct);
        }

        public override void Show()
        {
            base.Show();
            
            _currentIndexProp.Value = 0;
            View.ShowPage(_currentIndex);
        }
        
        /// <summary>
        /// 開いているページをアニメーションで閉じる
        /// </summary>
        /// <param name="ct"></param>
        public override async UniTask HideAsync(CancellationToken ct)
        {
            await View.HidePageAsync(_currentIndex, ct);
            
            base.Hide();
        }

        /// <summary>
        /// 開いているページを閉じる
        /// </summary>
        public override void Hide()
        {
            View.HidePage(_currentIndex);
            
            base.Hide();
        }
        
        /// <summary>
        /// 次のページを開く
        /// </summary>
        /// <param name="ct"></param>
        private async UniTask OpenNextPage(CancellationToken ct)
        {
            View.SetInteractable(false);
            
            View.HidePageAsync(_currentIndex, ct).Forget();
            await _bookWindowAnimation.PlayTurnRightAsync(ct);
            await ShowPageAsync(_currentIndex + 1, ct);
            
            View.SetInteractable(true);
        }

        /// <summary>
        /// 前のページを開く
        /// </summary>
        /// <param name="ct"></param>
        private async UniTask OpenPrePage(CancellationToken ct)
        {
            View.SetInteractable(false);
            
            View.HidePageAsync(_currentIndex, ct).Forget();
            await _bookWindowAnimation.PlayTurnLeftAsync(ct);
            await ShowPageAsync(_currentIndex - 1, ct);
            
            View.SetInteractable(true);
        }

        /// <summary>
        /// ページを表示する
        /// </summary>
        /// <param name="nextIndex"></param>
        /// <param name="ct"></param>
        private async UniTask ShowPageAsync(int nextIndex, CancellationToken ct)
        {
            _currentIndexProp.Value = Mathf.Clamp(nextIndex, 0, View.LastPageIndex);
            await View.ShowPageAsync(_currentIndex, ct);
        }
    }
}