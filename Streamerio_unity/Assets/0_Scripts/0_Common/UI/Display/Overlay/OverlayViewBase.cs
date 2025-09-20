using Alchemy.Inspector;
using Common.UI.Animation;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using System.Threading;
using Common.UI.Display.Background;
using UnityEngine;

namespace Common.UI.Display.Overlay
{
    public abstract class OverlayViewBase: DisplayViewBase
    {
        [SerializeField, LabelText("背景")]
        private DisplayBackgroundPresenter _background;
        public DisplayBackgroundPresenter Background => _background;

        [SerializeField, LabelText("アニメーションさせるオブジェクト")]
        private CanvasGroup[] _uiParts;
        
        [Header("アニメーション")]
        [SerializeField, LabelText("表示アニメーション")]
        private FadeAnimationComponentParam _showFadeAnimationParam = new ()
        {
            Alpha = 1f,
            Duration = 0.1f,
            Ease = Ease.InSine,
        };
        [SerializeField, LabelText("アニメーションの間")]
        private float _showDelay = 0.1f;
        
        [SerializeField, LabelText("非表示アニメーション")]
        private FadeAnimationComponentParam _hideFadeAnimationParam = new ()
        {
            Alpha = 0f,
            Duration = 0.1f,
            Ease = Ease.OutSine,
        };
        
        private FadeAnimationComponent[] _showAnimations;
        private FadeAnimationComponent _hideAnimation;

        public override void Initialize()
        {
            base.Initialize();
            
            _background.Initialize();
            
            _showAnimations = new FadeAnimationComponent[_uiParts.Length];
            for (int i = 0; i < _uiParts.Length; i++)
            {
                _showAnimations[i] = new FadeAnimationComponent(_uiParts[i], _showFadeAnimationParam);
            }
            
            _hideAnimation = new FadeAnimationComponent(CanvasGroup, _hideFadeAnimationParam);
        }
        
        public override async UniTask ShowAsync(CancellationToken ct)
        {
            await _background.ShowAsync(ct);
            
            for (int i = 0; i < _uiParts.Length; i++)
            {
                await _showAnimations[i].PlayAsync(ct);
                await UniTask.WaitForSeconds(_showDelay, cancellationToken: ct);
            }
        }
        
        public override void Show()
        {
            CanvasGroup.alpha = _showFadeAnimationParam.Alpha;
            _background.Show();
            SetAlphaParts(_showFadeAnimationParam.Alpha);
        }
        
        public override async UniTask HideAsync(CancellationToken ct)
        {
            await _hideAnimation.PlayAsync(ct);
            SetAlphaParts(_hideFadeAnimationParam.Alpha);
            await _background.HideAsync(ct);
        }

        public override void Hide()
        {
            CanvasGroup.alpha = _hideFadeAnimationParam.Alpha;
            _background.Hide();
            SetAlphaParts(_hideFadeAnimationParam.Alpha);
        }
        
        /// <summary>
        /// パーツの透明度を一括変更
        /// </summary>
        /// <param name="alpha"></param>
        private void SetAlphaParts(float alpha)
        {
            foreach (var part in _uiParts)
            {
                part.alpha = alpha;
            }
        }
    }
}