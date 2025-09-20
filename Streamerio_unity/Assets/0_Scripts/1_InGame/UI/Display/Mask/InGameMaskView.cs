using System.Threading;
using Alchemy.Inspector;
using Common.UI;
using Common.UI.Animation;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace InGame.UI.Displau.Mask
{
    /// <summary>
    /// インゲームのマスク
    /// </summary>
    public class InGameMaskView : UIBehaviourBase
    {
        [SerializeField, LabelText("マスク画像")]
        private Image _maskImage;
        
        [Header("アニメーション")]
        [SerializeField]
        private IrisAnimationComponentParam _irisAnimationParam;
        
        /// <summary>
        /// アニメーションで表示
        /// </summary>
        /// <param name="ct"></param>
        public async UniTask ShowAsync(Vector3 centerCirclePosition, CancellationToken ct)
        {
            SetInteractable(true);
            _maskImage.material.SetFloat(_irisAnimationParam.RadiusPropertyName, _irisAnimationParam.MaxRadius);
            CanvasGroup.alpha = 1f;
            
            var centerCircle = Camera.main.WorldToViewportPoint(centerCirclePosition);
            _irisAnimationParam.Center = centerCircle;
            
            var irisInAnimation = new IrisInAnimationComponent(_maskImage.material, _irisAnimationParam);
            await irisInAnimation.PlayAsync(ct);
            SetInteractable(false);
        }
        
        /// <summary>
        /// 非表示
        /// </summary>
        public void Hide()
        {
            CanvasGroup.alpha = 0;
            SetInteractable(false);
        }
    }   
}
