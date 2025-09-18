using System.Threading;
using Alchemy.Inspector;
using Common.UI.Animation;
using Common.UI.Part.Button;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

namespace Common.UI.Display.Window.Group
{
    public class WindowButtonGroup: UIBehaviourBase, IDisplay
    {
        [SerializeField, LabelText("閉じるボタン")]
        private CommonButton _closeButton;
        public CommonButton CloseButton => _closeButton;
        [SerializeField, LabelText("次のページボタン")]
        private CommonButton _nextButton;
        public CommonButton NextButton => _nextButton;
        [SerializeField, LabelText("前のページボタン")]
        private CommonButton _backButton;
        public CommonButton BackButton => _backButton;
        
        [Header("アニメーション")]
        [SerializeField, LabelText("表示アニメーション")]
        private FadeAnimationComponentParam _showAnimationParam = new ()
        {
            Alpha = 1f,
            Duration = 0.1f,
            Ease = Ease.InSine,
        };
        [SerializeField, LabelText("非表示アニメーション")]
        private FadeAnimationComponentParam _hideAnimationParam = new ()
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
            
            _closeButton.Initialize();
            _nextButton.Initialize();
            _backButton.Initialize();
            
            _showAnimation = new FadeAnimationComponent(CanvasGroup, _showAnimationParam);
            _hideAnimation = new FadeAnimationComponent(CanvasGroup, _hideAnimationParam);
        }
        
        public async UniTask ShowAsync(CancellationToken ct)
        {
            await _showAnimation.PlayAsync(ct);
        }
        
        public void Show()
        {
            CanvasGroup.alpha = _showAnimationParam.Alpha;
        }
        
        public async UniTask HideAsync(CancellationToken ct)
        {
            await _hideAnimation.PlayAsync(ct);
        }
        
        public void Hide()
        {
            CanvasGroup.alpha = _hideAnimationParam.Alpha;
        }
    }
}