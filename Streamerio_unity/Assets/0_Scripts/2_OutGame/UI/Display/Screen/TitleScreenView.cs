using System.Threading;
using Alchemy.Inspector;
using Common.UI;
using Common.UI.Animation;
using Common.UI.Display;
using Common.UI.Part.Text;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

namespace OutGame.UI.Display.Screen
{
    /// <summary>
    /// タイトル画面の見た目
    /// </summary>
    public class TitleScreenView: DisplayViewBase
    {
        [SerializeField]
        private FlashText _gameStartText;

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

        private CancellationTokenSource _cts;
        
        public override void Initialize()
        {
            base.Initialize();
            
            _gameStartText.Initialize();
            
            _showAnimation = new FadeAnimationComponent(CanvasGroup, _showFadeAnimationParam);
            _hideAnimation = new FadeAnimationComponent(CanvasGroup, _hideFadeAnimationParam);
        }
        
        public override async UniTask ShowAsync(CancellationToken ct)
        {
            await _showAnimation.PlayAsync(ct);
            _gameStartText.PlayTextAnimation();
        }
        
        public override void Show()
        {
            CanvasGroup.alpha = 1f;
            _gameStartText.PlayTextAnimation();
        }
        
        public override async UniTask HideAsync(CancellationToken ct)
        {
            await _hideAnimation.PlayAsync(ct);
            _gameStartText.StopTextAnimation();
        }
        
        public override void Hide()
        {
            CanvasGroup.alpha = 0f;
            _gameStartText.StopTextAnimation();
        }
    }
}