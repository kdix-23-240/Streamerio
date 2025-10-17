using UnityEngine;
using UnityEngine.EventSystems;

namespace Common.UI
{
    /// <summary>
    /// UI 共通のインターフェース。
    /// すべての UI 要素が最低限持つべきプロパティと処理を定義する。
    /// </summary>
    public interface ICommonUIBehaviour
    {
        /// <summary>
        /// この UI の RectTransform。
        /// レイアウトや位置制御で利用する。
        /// </summary>
        RectTransform RectTransform { get; }

        /// <summary>
        /// この UI の CanvasGroup。
        /// フェードやインタラクション制御で利用する。
        /// </summary>
        CanvasGroup CanvasGroup { get; }

        
        /// <summary>
        /// この UI のインタラクション有効／無効を切り替える。
        /// <para>
        /// - <c>true</c>：ボタンなどの操作を受け付ける  
        /// - <c>false</c>：入力を遮断し、操作不可にする
        /// </para>
        /// </summary>
        void SetInteractable(bool interactable);
        
        /// <summary>
        /// この UI の有効／無効を切り替える。
        /// </summary>
        void SetActive(bool active);
    }

    /// <summary>
    /// すべての UI コンポーネントの基底クラス。
    /// <para>
    /// - RectTransform / CanvasGroup を必須コンポーネントとして持つ  
    /// - インタラクション制御用のメソッドを提供  
    /// - 共通の初期化／参照補完処理を集約
    /// </para>
    /// </summary>
    [RequireComponent(typeof(RectTransform), typeof(CanvasGroup))]
    public abstract class UIBehaviourBase : UIBehaviour, ICommonUIBehaviour
    {
        [SerializeField, Alchemy.Inspector.ReadOnly]
        private GameObject _gameObject;
        public GameObject GameObject => _gameObject;
        [SerializeField, Alchemy.Inspector.ReadOnly]
        private RectTransform _rectTransform;
        public RectTransform RectTransform => _rectTransform;

        [SerializeField, Alchemy.Inspector.ReadOnly]
        private CanvasGroup _canvasGroup;
        public CanvasGroup CanvasGroup => _canvasGroup;

#if UNITY_EDITOR
        /// <summary>
        /// エディタ上で値が変更された際、自動的に参照を補完する。
        /// （手動でアタッチし忘れても安全に補正可能）
        /// </summary>
        protected override void OnValidate()
        {
            base.OnValidate();
            _gameObject = gameObject;
            _rectTransform = GetComponent<RectTransform>();
            _canvasGroup = GetComponent<CanvasGroup>();
        }
#endif

        /// <inheritdoc/>
        public void SetInteractable(bool interactable)
        {
            _canvasGroup.interactable = interactable;
            _canvasGroup.blocksRaycasts = interactable;
        }

        /// <inheritdoc/>
        public virtual void SetActive(bool active)
        {
            _gameObject.SetActive(active);
        }
    }
}
