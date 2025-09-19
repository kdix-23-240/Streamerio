using System.Threading;
using Alchemy.Inspector;
using Common.UI.Animation;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

namespace Common.UI.Display.Background
{
    public class DisplayBackgroundView: DisplayViewBase
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
        
        public override void Initialize()
        {
            base.Initialize();
            
            _showAnimation = new FadeAnimationComponent(CanvasGroup, _showFadeAnimationParam);
            _hideAnimation = new FadeAnimationComponent(CanvasGroup, _hideFadeAnimationParam);
        }
        
        public override async UniTask ShowAsync(CancellationToken ct)
        {
            await _showAnimation.PlayAsync(ct);
        }

        public override void Show()
        {
            CanvasGroup.alpha = _showFadeAnimationParam.Alpha;
        }

        public override async UniTask HideAsync(CancellationToken ct)
        {
            await _hideAnimation.PlayAsync(ct);
        }

        public override void Hide()
        {
            CanvasGroup.alpha = _hideFadeAnimationParam.Alpha;
        }
    }
}