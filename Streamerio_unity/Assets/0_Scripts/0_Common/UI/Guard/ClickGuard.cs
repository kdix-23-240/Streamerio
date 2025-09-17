using Alchemy.Inspector;
using UnityEngine;

namespace Common.UI.Guard
{
    /// <summary>
    /// クリックをガードする
    /// </summary>
    [RequireComponent(typeof(CanvasGroup))]
    public class ClickGuard: SingletonBase<ClickGuard>
    {
        [SerializeField, ReadOnly]
        private CanvasGroup _canvasGroup;

#if UNITY_EDITOR
        private void OnValidate()
        {
            _canvasGroup ??= GetComponent<CanvasGroup>();
        }
#endif
        
        /// <summary>
        /// 初期化
        /// </summary>
        public void Initialize()
        {
            Guard(false);
        }
        
        /// <summary>
        /// UIを触れるようにするか設定
        /// </summary>
        /// <param name="isGuard">触れる/触れない</param>
        public void Guard(bool isGuard)
        {
            _canvasGroup.interactable = isGuard;
            _canvasGroup.blocksRaycasts = isGuard;
        }
    }
}