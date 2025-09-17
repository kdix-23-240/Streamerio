using System.Threading;
using Alchemy.Inspector;
using Common.UI.Animation;
using Common.UI.Display.Background;
using Common.UI.Display.Window.Animation;
using Common.UI.Display.Window.Group;
using Common.UI.Display.Window.Page;
using Common.UI.Guard;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using R3;

namespace Common.UI.Display.Window
{
    public abstract class WindowPresenterBase<TView>: DisplayPresenterBase<TView>
        where TView: WindowViewBase
    {
        protected override void SetEvent()
        {
            base.SetEvent();
            
            View.ButtonGroup.CloseButton
                .SetClickEvent(() =>
                {
                    Debug.Log("Close Button Clicked");
                });
            
            View.ButtonGroup.NextButton
                .SetClickEvent(async () =>
                {
                    ClickGuard.Instance.Guard(true);
                    await View.NextPageAsync(destroyCancellationToken);
                    ClickGuard.Instance.Guard(false);
                });
            
            View.ButtonGroup.BackButton
                .SetClickEvent(async () =>
                {
                    ClickGuard.Instance.Guard(true);
                    await View.BackPageAsync(destroyCancellationToken);
                    ClickGuard.Instance.Guard(false);
                });
        }

        protected override void Bind()
        {
            base.Bind();
            
            View.Background.OnClickAsObservable
                .Subscribe(_ =>
                {
                   Debug.Log("Background Clicked"); 
                }).RegisterTo(destroyCancellationToken);
        }
    }
    
    [RequireComponent(typeof(BookWindowAnimation))]
    public abstract class WindowViewBase: DisplayViewBase
    {
        [SerializeField, LabelText("背景"), ReadOnly]
        private DisplayBackground _background;
        public DisplayBackground Background => _background;

        [SerializeField, LabelText("ページ(表示順)")]
        private PagePresenter[] _pages;
        
        [SerializeField, LabelText("ボタン")]
        private WindowButtonGroup _buttonGroup;
        public WindowButtonGroup ButtonGroup => _buttonGroup;
        
        [Header("アニメション")]
        [SerializeField, LabelText("表示アニメーション")]
        private MoveAnimationComponentParam _showAnimParam = new()
        {
            Position = Vector2.zero,
            Duration = 0.2f,
            Ease = Ease.InSine,
        };
        [SerializeField, LabelText("非表示アニメーション")]
        private MoveAnimationComponentParam _hideAnimParam = new()
        {
            Position = Vector2.zero,
            Duration = 0.2f,
            Ease = Ease.OutSine,
        };

        [SerializeField, LabelText("本のアニメーション"), ReadOnly]
        private BookWindowAnimation _bookWindowAnimation;
        
        private MoveAnimationComponent _showAnim;
        private MoveAnimationComponent _hideAnim;
        
        private int _currentPageIndex = 0;

#if UNITY_EDITOR
        private void OnValidate()
        {
            _background ??= GetComponentInChildren<DisplayBackground>();
            _bookWindowAnimation ??= GetComponent<BookWindowAnimation>();
        }
#endif
        
        public override void Initialize()
        {
            base.Initialize();
            
            _background.Initialize();
            _background.Hide();
            
            _buttonGroup.Initialize();
            _buttonGroup.Hide();

            _showAnim = new MoveAnimationComponent(RectTransform, _showAnimParam);
            _hideAnim = new MoveAnimationComponent(RectTransform, _hideAnimParam);

            _bookWindowAnimation.Initialize();

            foreach (var page in _pages)
            {
                page.Initialize();
                page.Hide();
            }
        }

        public override async UniTask ShowAsync(CancellationToken ct)
        {
            _background.Show();
            
            await _showAnim.PlayAsync(ct);
            
            _currentPageIndex = 0;
            await _pages[_currentPageIndex].ShowAsync(ct);
            
            await _buttonGroup.ShowAsync(_currentPageIndex == 0, _currentPageIndex == _pages.Length - 1, ct);
        }

        public override void Show()
        {
            _background.Show();
            
            RectTransform.anchoredPosition = _showAnimParam.Position;
            
            _currentPageIndex = 0;
            _pages[_currentPageIndex].Show();
            
            _buttonGroup.Show(_currentPageIndex == 0, _currentPageIndex == _pages.Length - 1);
        }

        public override async UniTask HideAsync(CancellationToken ct)
        {
            _buttonGroup.HideAsync(ct).Forget();
            
            await _pages[_currentPageIndex].HideAsync(ct);
            
            await _hideAnim.PlayAsync(ct);
            
            _background.Hide();
        }

        public override void Hide()
        {
            _buttonGroup.Hide();
            
            _pages[_currentPageIndex].Hide();
            
            RectTransform.anchoredPosition = _hideAnimParam.Position;
            
            _background.Hide();
        }

        /// <summary>
        /// 次のページをめくる
        /// </summary>
        /// <param name="ct"></param>
        public virtual async UniTask NextPageAsync(CancellationToken ct)
        {
            await CloseCurrentPageAsync(ct);
            
            await _bookWindowAnimation.PlayTurnRightAsync(ct);

            await OpenPageAsync(_currentPageIndex + 1, ct);
        }
        
        /// <summary>
        /// 前のページをめくる
        /// </summary>
        /// <param name="ct"></param>
        public virtual async UniTask BackPageAsync(CancellationToken ct)
        {
            await CloseCurrentPageAsync(ct);
            
            await _bookWindowAnimation.PlayTurnLeftAsync(ct);
            
            await OpenPageAsync(_currentPageIndex - 1, ct);
        }
        
        /// <summary>
        /// 開いているページを閉じる
        /// </summary>
        /// <param name="ct"></param>
        private async UniTask CloseCurrentPageAsync(CancellationToken ct)
        {
            _buttonGroup.HideAsync(ct).Forget();
            await _pages[_currentPageIndex].HideAsync(ct);
        }
        
        /// <summary>
        /// ページを開く
        /// </summary>
        /// <param name="index"></param>
        /// <param name="ct"></param>
        private async UniTask OpenPageAsync(int index, CancellationToken ct)
        {
            _currentPageIndex = Mathf.Clamp(index, 0, _pages.Length - 1);
            await _pages[_currentPageIndex].ShowAsync(ct);
            await _buttonGroup.ShowAsync(_currentPageIndex == 0, _currentPageIndex == _pages.Length - 1, ct);
        }
    }
}