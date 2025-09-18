using UnityEngine;
using UnityEngine.EventSystems;

namespace Common.UI
{
    /// <summary>
    /// UIの共通インターフェース
    /// </summary>
    public interface ICommonUIBehaviour
    {
        RectTransform RectTransform { get; }
        CanvasGroup CanvasGroup { get; }

        /// <summary>
        /// 初期化
        /// </summary>
        void Initialize();
    }
    
    /// <summary>
    /// UIの基底クラス
    /// </summary>
    [RequireComponent(typeof(RectTransform), typeof(CanvasGroup))]
    public abstract class UIBehaviourBase: UIBehaviour, ICommonUIBehaviour
    {
        [SerializeField, Alchemy.Inspector.ReadOnly]
        private RectTransform _rectTransform;

        public RectTransform RectTransform => _rectTransform;
        [SerializeField, Alchemy.Inspector.ReadOnly]
        private CanvasGroup _canvasGroup;

        public CanvasGroup CanvasGroup => _canvasGroup;

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();

            _rectTransform = GetComponent<RectTransform>();
            _canvasGroup = GetComponent<CanvasGroup>();
        }  
#endif

        public virtual void Initialize()
        {
            
        }
        
        /// <summary>
        /// インタラクティブ設定
        /// </summary>
        /// <param name="interactable"></param>
        public void SetInteractable(bool interactable)
        {
            _canvasGroup.interactable = interactable;
            _canvasGroup.blocksRaycasts = interactable;
        }
    }
}