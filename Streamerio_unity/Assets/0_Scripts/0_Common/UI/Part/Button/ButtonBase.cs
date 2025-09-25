using Alchemy.Inspector;
using Common.UI.Click;
using R3;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Common.UI.Part.Button
{
    /// <summary>
    /// ボタンの基底クラス
    /// </summary>
    [RequireComponent(typeof(UnityEngine.UI.Button), typeof(ClickEventBinder))]
    public abstract class ButtonBase: UIBehaviourBase, IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField, ReadOnly]
        private UnityEngine.UI.Button _button;
        [SerializeField, ReadOnly]
        private ClickEventBinder _clickEventBinder;
        
        /// <summary>
        /// クリックした時の処理
        /// </summary>
        public Observable<Unit> OnClickAsObservable => _clickEventBinder.ClickEvent; 

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();

            _button ??= GetComponent<UnityEngine.UI.Button>();
            _clickEventBinder ??= GetComponent<ClickEventBinder>();
        }
#endif

        public override void Initialize()
        {
            _clickEventBinder.Initialize();
            base.Initialize();
            Bind();
        }

        private void Bind()
        {
            _clickEventBinder.BindClickEvent(_button.OnClickAsObservable());
            
            OnClickAsObservable
                .Subscribe(_ =>
                {
                    ResetButtonState();
                })
                .RegisterTo(destroyCancellationToken);
        }

        public abstract void OnPointerDown(PointerEventData eventData);

        public abstract void OnPointerUp(PointerEventData eventData);

        public abstract void OnPointerEnter(PointerEventData eventData);

        public abstract void OnPointerExit(PointerEventData eventData);
        
        /// <summary>
        /// ボタンをデフォルトの見た目に戻す
        /// </summary>
        protected abstract void ResetButtonState();
        
        /// <summary>
        /// ボタンの操作可能/不可能を設定
        /// </summary>
        /// <param name="isInteractable"></param>
        public void SetInteractable(bool isInteractable)
        {
            _button.interactable = isInteractable;
        }
    }
}