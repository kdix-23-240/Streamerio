using System.Threading;
using Alchemy.Inspector;
using Common.UI;
using Common.UI.Animation;
using Common.UI.Display;
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
        private CanvasGroup _gameStartText;

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
        [SerializeField, LabelText("テキストの点滅アニメーション")]
        private FlashAnimationComponentParam _flashAnimationParam;
        
        private FadeAnimationComponent _showAnimation;
        private FadeAnimationComponent _hideAnimation;
        private FlashAnimationComponent _flashAnimation;

        private CancellationTokenSource _cts;
        
        public override void Initialize()
        {
            base.Initialize();
            
            _showAnimation = new FadeAnimationComponent(CanvasGroup, _showFadeAnimationParam);
            _hideAnimation = new FadeAnimationComponent(CanvasGroup, _hideFadeAnimationParam);
            _flashAnimation = new FlashAnimationComponent(_gameStartText, _flashAnimationParam);
        }
        
        public override async UniTask ShowAsync(CancellationToken ct)
        {
            await _showAnimation.PlayAsync(ct);
            PlayStartTextAnimation();
        }
        
        public override void Show()
        {
            CanvasGroup.alpha = 1f;
            PlayStartTextAnimation();
        }
        
        public override async UniTask HideAsync(CancellationToken ct)
        {
            await _hideAnimation.PlayAsync(ct);
            StopStartTextAnimation();
        }
        
        public override void Hide()
        {
            CanvasGroup.alpha = 0f;
            StopStartTextAnimation();
        }
        
        /// <summary>
        /// テキストのアニメーション再生
        /// </summary>
        private void PlayStartTextAnimation()
        {
            _cts = new CancellationTokenSource();
            _flashAnimation.PlayAsync(_cts.Token).Forget();
        }
        
        /// <summary>
        /// テキストのアニメーションを止める
        /// </summary>
        private void StopStartTextAnimation()
        {
            _cts.Cancel();
            _cts.Dispose();
            
            _gameStartText.alpha = _flashAnimationParam.MaxAlpha;
        }
    }
}