using Alchemy.Inspector;
using Common.UI.Animation;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Common.UI.Part.Button
{
    /// <summary>
    /// テキストのボタン
    /// </summary>
    public class TextButton: ButtonBase
    {
        [Header("画像")]
        [SerializeField, LabelText("羽")]
        private Image _featherImage;
        [SerializeField, LabelText("下線")]
        private Image _lineImage;
        
        [SerializeField, LabelText("ボタンを押した時のアニメーション")]
        private ScaleAnimationComponentParam _pushDownAnimParam = new()
        {
            Scale = 0.75f, 
            Duration = 0.1f,
            Ease = Ease.InSine,
        };
        [SerializeField, LabelText("ボタンを離した時のアニメーション")]
        private ScaleAnimationComponentParam _pushUpAnimParam = new()
        {
            Scale = 1f, 
            Duration = 0.1f,
            Ease = Ease.OutSine,
        };
        
        private ScaleAnimationComponent _pushDownAnim;
        private ScaleAnimationComponent _pushUpAnim;

        public override void Initialize()
        {
            base.Initialize();
            
            _featherImage.enabled = false;
            _lineImage.enabled = false;
            
            _pushDownAnim = new ScaleAnimationComponent(RectTransform, _pushDownAnimParam);
            _pushUpAnim = new ScaleAnimationComponent(RectTransform, _pushUpAnimParam);
        }

        public override void OnPointerDown(PointerEventData eventData)
        {
            _pushDownAnim.PlayAsync(destroyCancellationToken).Forget();
        }
        
        public override void OnPointerUp(PointerEventData eventData)
        {
            _pushUpAnim.PlayAsync(destroyCancellationToken).Forget();
        }
        
        public override void OnPointerEnter(PointerEventData eventData)
        {
            _featherImage.enabled = true;
            _lineImage.enabled = true;
        }
        
        public override void OnPointerExit(PointerEventData eventData)
        {
            _pushUpAnim.PlayAsync(destroyCancellationToken).Forget();
            
            _featherImage.enabled = false;
            _lineImage.enabled = false;
        }
        
        protected override void ResetButtonState()
        {
            RectTransform.localScale = _pushDownAnimParam.Scale* Vector3.one;
            
            _featherImage.enabled = false;
            _lineImage.enabled = false;
        }
    }
}