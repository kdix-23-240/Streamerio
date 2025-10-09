using System.Threading;
using Common.UI.Animation;
using Cysharp.Threading.Tasks;
using UnityEngine;
using VContainer.Unity;

namespace Common.UI.Display.Background
{
    /// <summary>
    /// UI 背景の View。
    /// - CanvasGroup を用いたフェードアニメーションで表示/非表示を制御
    /// - 即時表示/非表示にも対応
    /// </summary>
    public class DisplayBackgroundView : DisplayViewBase, IInitializable
    {
        [SerializeField]
        private FadeAnimationComponentParamSO _showAnimationParam;
        [SerializeField]
        private FadeAnimationComponentParamSO _hideAnimationParam;
        
        private IUIAnimationComponent _showAnimation;
        private IUIAnimationComponent _hideAnimation;

        public void Initialize()
        {
            _showAnimation = new FadeAnimationComponent(CanvasGroup, _showAnimationParam);
            _hideAnimation = new FadeAnimationComponent(CanvasGroup, _hideAnimationParam);
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
