using UnityEngine;
using UnityEngine.EventSystems;

namespace Common.UI
{
    /// <summary>
    /// UI 共通のインターフェース。
    /// すべての UI 要素が最低限持つべきプロパティや処理を定義する。
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
        /// フェード制御やインタラクション制御で利用する。
        /// </summary>
        CanvasGroup CanvasGroup { get; }

        /// <summary>
        /// 初期化処理。
        /// 継承先で必要な準備を行う。
        /// </summary>
        void Initialize();
    }
    
    /// <summary>
    /// すべての UI コンポーネントの基底クラス。
    /// - RectTransform / CanvasGroup を必須コンポーネントとして持つ
    /// - インタラクション制御用のメソッドを提供
    /// - 共通初期化処理をまとめるためのベース
    /// </summary>
    [RequireComponent(typeof(RectTransform), typeof(CanvasGroup))]
    public abstract class UIBehaviourBase : UIBehaviour, ICommonUIBehaviour
    {
        [SerializeField, Alchemy.Inspector.ReadOnly]
        private RectTransform _rectTransform;
        public RectTransform RectTransform => _rectTransform;

        [SerializeField, Alchemy.Inspector.ReadOnly]
        private CanvasGroup _canvasGroup;
        public CanvasGroup CanvasGroup => _canvasGroup;

#if UNITY_EDITOR
        /// <summary>
        /// エディタ上で値が変更されたときに、自動でコンポーネント参照を補完する。
        /// </summary>
        protected override void OnValidate()
        {
            base.OnValidate();
            _rectTransform = GetComponent<RectTransform>();
            _canvasGroup = GetComponent<CanvasGroup>();
        }  
#endif

        /// <summary>
        /// 初期化処理。
        /// デフォルトでは何もしないが、継承先でオーバーライドして利用する。
        /// </summary>
        public virtual void Initialize() { }
        
        /// <summary>
        /// この UI のインタラクション有効/無効を切り替える。
        /// - interactable: true の場合 → ボタン等の操作を受け付ける
        /// - false の場合 → 入力を遮断する
        /// </summary>
        public void SetInteractable(bool interactable)
        {
            _canvasGroup.interactable = interactable;
            _canvasGroup.blocksRaycasts = interactable;
        }
    }
}