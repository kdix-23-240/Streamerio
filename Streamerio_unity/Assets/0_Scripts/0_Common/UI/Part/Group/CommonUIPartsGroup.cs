using System.Threading;
using Alchemy.Inspector;
using Common.UI.Animation;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Common.UI.Part.Group
{
    public class CommonUIPartsGroup: UIBehaviour
    {
        [SerializeField, LabelText("アニメーションさせるオブジェクト")]
        private CanvasGroup[] _uiParts;
        
        [Header("アニメーション")]
        [SerializeField, LabelText("表示アニメーション")]
        private FadePartsAnimationComponentParam _showFadeAnimationParam = new ()
        {
            Alpha = 1f,
            DurationSec = 0.15f,
            Ease = Ease.InSine,
            ShowDelaySec = 0.1f
        };
        
        [SerializeField, LabelText("非表示のアニメーション")]
        private FadePartsAnimationComponentParam _hideFadeAnimationParam = new ()
        {
            Alpha = 0f,
            DurationSec = 0.1f,
            Ease = Ease.OutSine,
            ShowDelaySec = 0.05f
        };  
        
        private FadePartsAnimationComponent _showAnimations;
        private FadePartsAnimationComponent _hideAnimation;

        public void Initialize()
        {
            _showAnimations = new FadePartsAnimationComponent(_uiParts, _showFadeAnimationParam);
            _hideAnimation = new FadePartsAnimationComponent(_uiParts, _hideFadeAnimationParam);
        }
        
        public async UniTask ShowAsync(CancellationToken ct)
        {
            await _showAnimations.PlayAsync(ct);
        }

        public void Show()
        {
            SetAlphaParts(_showFadeAnimationParam.Alpha);
        }
        
        public async UniTask HideAsync(CancellationToken ct)
        {
            await _hideAnimation.PlayAsync(ct);
        }
        
        public void Hide()
        {
            SetAlphaParts(_hideFadeAnimationParam.Alpha);
        }
        
        /// <summary>
        /// 全 UI パーツの透明度を一括変更
        /// </summary>
        private void SetAlphaParts(float alpha)
        {
            foreach (var part in _uiParts)
            {
                part.alpha = alpha;
            }
        }
    }
}