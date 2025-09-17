using UnityEngine;
using UnityEngine.EventSystems;

namespace Common.UI
{
    public abstract class PresenterBase<TView>: UIBehaviour
        where TView: ICommonUIBehaviour
    {
        [SerializeField]
        protected TView View;

#if UNITY_EDITOR
        private void OnValidate()
        {
            View ??= GetComponent<TView>();
        }
#endif
        
        /// <summary>
        /// 初期化
        /// </summary>
        public virtual void Initialize()
        {
            SetEvent();
            Bind();
        }

        /// <summary>
        /// イベントの設定
        /// </summary>
        protected virtual void SetEvent()
        {
            
        }

        /// <summary>
        /// イベントの焼き付け
        /// </summary>
        protected virtual void Bind()
        {
            
        }
    }
}