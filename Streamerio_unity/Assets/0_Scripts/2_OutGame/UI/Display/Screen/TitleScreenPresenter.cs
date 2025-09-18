using Alchemy.Inspector;
using Common.UI.Guard;
using Cysharp.Threading.Tasks;
using OutGame.Title;
using OutGame.UI.Display.Screen;
using UnityEngine;
using UnityEngine.EventSystems;

namespace _0_Scripts._2_OutGame.UI.Display.Screen
{
    /// <summary>
    /// タイトルのスクリーンのつなぎ役
    /// </summary>
    [RequireComponent(typeof(TitleScreenView))]
    public class TitleScreenPresenter: UIBehaviour, IPointerClickHandler
    {
        [SerializeField, ReadOnly]
        private TitleScreenView _view;
        
#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();
            _view ??= GetComponent<TitleScreenView>();
        }
#endif
        
        /// <summary>
        /// 初期化
        /// </summary>
        public void Initialize()
        {
            _view.Initialize();
        }

        /// <summary>
        /// 非表示
        /// </summary>
        public void Show()
        {
            ClickGuard.Instance.Guard(true);
            _view.SetInteractable(true);
            _view.Show();
            ClickGuard.Instance.Guard(false);
        }
        
        /// <summary>
        /// 表示
        /// </summary>
        public void Hide()
        {
            ClickGuard.Instance.Guard(true);
            _view.Hide();
            _view.SetInteractable(false);
            ClickGuard.Instance.Guard(false);
        }
        
        public void OnPointerClick(PointerEventData eventData)
        {
            Hide();
            TitleManager.Instance.OpenTitleWindowAsync().Forget();
        }
    }
}