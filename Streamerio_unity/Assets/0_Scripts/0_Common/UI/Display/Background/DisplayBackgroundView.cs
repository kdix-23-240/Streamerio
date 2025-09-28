using System.Threading;
using Alchemy.Inspector;
using Common.UI.Animation;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

namespace Common.UI.Display.Background
{
    /// <summary>
    /// UI 背景の View。
    /// - CanvasGroup を用いたフェードアニメーションで表示/非表示を制御
    /// - 即時表示/非表示にも対応
    /// </summary>
    public class DisplayBackgroundView : DisplayViewBase
    {
        [Header("アニメーション")]
        [SerializeField, LabelText("表示アニメーション")]
        private FadeAnimationComponentParam _showFadeAnimationParam = new ()
        {
            Alpha = 1f,
            Duration = 0.1f,
            Ease = Ease.InSine,
        };

        [SerializeField, LabelText("非表示アニメーション")]
        private FadeAnimationComponentParam _hideFadeAnimationParam = new ()
        {
            Alpha = 0f,
            Duration = 0.1f,
            Ease = Ease.OutSine,
        };
        
        private FadeAnimationComponent _showAnimation;
        private FadeAnimationComponent _hideAnimation;
        
        /// <summary>
        /// 初期化処理。
        /// - フェードイン/フェードアウト用のアニメーションコンポーネントを生成
        /// </summary>
        public override void Initialize()
        {
            base.Initialize();
            
            _showAnimation = new FadeAnimationComponent(CanvasGroup, _showFadeAnimationParam);
            _hideAnimation = new FadeAnimationComponent(CanvasGroup, _hideFadeAnimationParam);
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
            CanvasGroup.alpha = _showFadeAnimationParam.Alpha;
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
            CanvasGroup.alpha = _hideFadeAnimationParam.Alpha;
        }
    }
}
