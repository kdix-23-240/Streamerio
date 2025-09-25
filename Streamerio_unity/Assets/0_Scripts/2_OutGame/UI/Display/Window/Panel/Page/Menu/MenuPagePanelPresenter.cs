using Alchemy.Inspector;
using Common.Scene;
using Common.UI.Display.Window.Panel;
using Common.UI.Loading;
using Common.UI.Part.Button;
using Cysharp.Threading.Tasks;
using OutGame.Title;
using R3;
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

            _menuPanelView.StartButton.OnClickAsObservable
                .SubscribeAwait(async (_, ct) =>
                {
                    await TitleManager.Instance.LoadTitleAsync();
                    WebsocketManager.Instance.ConnectWebSocket();
                    SceneManager.Instance.LoadSceneAsync(SceneType.GameScene).Forget();
                }).RegisterTo(destroyCancellationToken);

            BindChapterButton(_menuPanelView.HowToPlayButton, ChapterType.HowToPlay);
            BindChapterButton(_menuPanelView.OptionButton, ChapterType.Option);
            BindChapterButton(_menuPanelView.CreditButton, ChapterType.Credit);

            _menuPanelView.ExitButton.OnClickAsObservable
                .Subscribe(_ =>
                {
                    TitleManager.Instance.ShowTitleAsync(destroyCancellationToken).Forget();
                }).RegisterTo(destroyCancellationToken);
        }

        /// <summary>
        /// チャプターパネルを開くイベントをボタンに焼き付け
        /// </summary>
        /// <param name="button"></param>
        /// <param name="type"></param>
        private void BindChapterButton(ButtonBase button, ChapterType type)
        {
            button.OnClickAsObservable
                .Subscribe(_=>
                {
                    ChapterManager.Instance.OpenChapterAsync(type, destroyCancellationToken).Forget();
                }).RegisterTo(destroyCancellationToken);
        }
    }
}