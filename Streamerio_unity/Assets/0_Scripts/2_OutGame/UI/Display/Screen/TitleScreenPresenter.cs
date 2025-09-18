using Alchemy.Inspector;
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
            _view.SetInteractable(true);
            _view.Show();
        }
        
        /// <summary>
        /// 表示
        /// </summary>
        public void Hide()
        {
            _view.Hide();
            _view.SetInteractable(false);
        }
        
        public void OnPointerClick(PointerEventData eventData)
        {
            Hide();
            TitleManager.Instance.OpenTitleWindowAsync(destroyCancellationToken).Forget();
        }
    }
}