using Alchemy.Inspector;
using Common.UI.Animation;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using R3;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace Common.UI.Part.Button
{
    /// <summary>
    /// 共通のボタン
    /// </summary>
    public class CommonButton: UIBehaviourBase, IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField, Alchemy.Inspector.ReadOnly]
        private UnityEngine.UI.Button _button;

        [SerializeField, LabelText("ボタンに押した時のアニメーション")]
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
        [SerializeField, LabelText("ボタンにカーソルがあった時のアニメーション")]
        private FadeAnimationComponentParam _enterAnimParam = new ()
        {
            Alpha = 0.5f,
            Duration = 0.1f,
            Ease = Ease.InSine,
        };
        [SerializeField, LabelText("ボタンにカーソルが離れた時のアニメーション")]
        private FadeAnimationComponentParam _exitAnimParam = new()
        {
            Alpha = 1f,
            Duration = 0.1f,
            Ease = Ease.OutSine,
        };

        private ScaleAnimationComponent _pushDownAnim;
        private ScaleAnimationComponent _pushUpAnim;
        private FadeAnimationComponent _enterAnim;
        private FadeAnimationComponent _exitAnim;

        /// <summary>
        /// クリックした時のイベント
        /// </summary>
        public Observable<Unit> ClickEventObservable => _button.OnClickAsObservable();

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();

            _button ??= GetComponent<UnityEngine.UI.Button>();
        }
#endif
        
        public override void Initialize()
        {
            base.Initialize();

            _pushDownAnim = new ScaleAnimationComponent(RectTransform, _pushDownAnimParam);
            _pushUpAnim = new ScaleAnimationComponent(RectTransform, _pushUpAnimParam);
            _enterAnim = new FadeAnimationComponent(CanvasGroup, _enterAnimParam);
            _exitAnim = new FadeAnimationComponent(CanvasGroup, _exitAnimParam);
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            _pushDownAnim.PlayAsync(destroyCancellationToken).Forget();            
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            _pushUpAnim.PlayAsync(destroyCancellationToken).Forget();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            _enterAnim.PlayAsync(destroyCancellationToken).Forget();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _exitAnim.PlayAsync(destroyCancellationToken).Forget();
            _pushUpAnim.PlayAsync(destroyCancellationToken).Forget();
        }
    }
}