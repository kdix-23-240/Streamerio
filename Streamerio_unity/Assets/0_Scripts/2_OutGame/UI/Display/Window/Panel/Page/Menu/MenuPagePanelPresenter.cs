using Alchemy.Inspector;
using Common.UI.Display.Window.Panel;
using Common.UI.Part.Button;
using Cysharp.Threading.Tasks;
using OutGame.Title;
using UnityEngine;

namespace OutGame.UI.Display.Window.Panel.Page.Menu
{
    [RequireComponent(typeof(MenuPanelView))]
    public class MenuPagePanelPresenter: PagePanelPresenter
    {
        [SerializeField, ReadOnly]
        private MenuPanelView _menuPanelView;

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();

            _menuPanelView ??= GetComponent<MenuPanelView>();
        }
#endif
        
        public override void Initialize()
        {
            _menuPanelView.Initialize();
            base.Initialize();
        }

        protected override void Bind()
        {
            base.Bind();

            _menuPanelView.StartButton.SetClickEvent(()=> Debug.Log("Game Start"));

            BindChapterButton(_menuPanelView.HowToPlayButton, ChapterType.HowToPlay);
            BindChapterButton(_menuPanelView.OptionButton, ChapterType.Option);
            BindChapterButton(_menuPanelView.CreditButton, ChapterType.Credit);

            _menuPanelView.ExitButton.SetClickEvent(() => TitleManager.Instance.ShowTitleAsync(destroyCancellationToken).Forget());
        }

        /// <summary>
        /// チャプターパネルを開くイベントをボタンに焼き付け
        /// </summary>
        /// <param name="button"></param>
        /// <param name="type"></param>
        private void BindChapterButton(ButtonBase button, ChapterType type)
        {
            button.SetClickEvent(()=>ChapterManager.Instance.OpenChapterAsync(type, destroyCancellationToken).Forget());
        }
    }
}