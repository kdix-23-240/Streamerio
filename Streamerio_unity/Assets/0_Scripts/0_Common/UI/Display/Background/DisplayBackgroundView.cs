using System.Threading;
using Common.UI.Animation;
using Cysharp.Threading.Tasks;
using VContainer;

namespace Common.UI.Display.Background
{
    /// <summary>
    /// UI 背景の View。
    /// - CanvasGroup を用いたフェードアニメーションで表示/非表示を制御
    /// - 即時表示/非表示にも対応
    /// </summary>
    public class DisplayBackgroundView : DisplayViewBase
    {
        private IUIAnimationComponent _showAnimation;
        private IUIAnimationComponent _hideAnimation;
        
        [Inject]
        public void Construct([Key(AnimationType.Show)] IUIAnimationComponent showAnimation,
                                     [Key(AnimationType.Hide)] IUIAnimationComponent hideAnimation)
        {
            _showAnimation = showAnimation;
            _hideAnimation = hideAnimation;
        }
        
        /// <summary>
        /// アニメーション付きで背景を表示。
        /// </summary>
        public override async UniTask ShowAsync(CancellationToken ct)
        {
            await _showAnimation.PlayAsync(ct);
        }

        /// <summary>
        /// 即時表示。
        /// </summary>
        public override void Show()
        {
            CanvasGroup.alpha = UIUtil.DEFAULT_SHOW_ALPHA;
        }

        /// <summary>
        /// アニメーション付きで背景を非表示。
        /// </summary>
        public override async UniTask HideAsync(CancellationToken ct)
        {
            await _hideAnimation.PlayAsync(ct);
        }

        /// <summary>
        /// 即時非表示。
        /// </summary>
        public override void Hide()
        {
            CanvasGroup.alpha = UIUtil.DEFAULT_HIDE_ALPHA;
        }
    }
}
