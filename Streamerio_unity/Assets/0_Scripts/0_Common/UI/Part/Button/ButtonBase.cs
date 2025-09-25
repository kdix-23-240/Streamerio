using System;
using Alchemy.Inspector;
using Common.Audio;
using Cysharp.Threading.Tasks;
using R3;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace Common.UI.Part.Button
{
    /// <summary>
    /// ボタンの基底クラス
    /// </summary>
    public abstract class ButtonBase: UIBehaviourBase, IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField, ReadOnly]
        private UnityEngine.UI.Button _button;
        [SerializeField, LabelText("SE")]
        private SEType _seType = SEType.SNESRPG01;
        [SerializeField, LabelText("ボタンのクリック間隔(秒)")]
        private float _clickIntervalSec = 0.1f;

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();

            _button ??= GetComponent<UnityEngine.UI.Button>();
        }
#endif
        
        /// <summary>
        /// イベント設定
        /// </summary>
        public void SetClickEvent(UnityAction onClick)
        {
            _button.OnClickAsObservable()
                .ThrottleFirst(TimeSpan.FromSeconds(_clickIntervalSec))
                .Subscribe(_ =>
                {
                    AudioManager.Instance.PlayAsync(_seType, destroyCancellationToken).Forget();
                    onClick?.Invoke();
                    ResetButtonState();
                }).RegisterTo(destroyCancellationToken);
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