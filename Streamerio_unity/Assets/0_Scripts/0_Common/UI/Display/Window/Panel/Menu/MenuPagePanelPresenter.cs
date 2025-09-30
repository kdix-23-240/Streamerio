using Alchemy.Inspector;
using Common.Scene;
using Cysharp.Threading.Tasks;
using OutGame.Title;
using R3;
using UnityEngine;

namespace Common.UI.Display.Window.Panel
{
    [RequireComponent(typeof(MenuPanelView))]
    public class MenuPagePanelPresenter: PagePanelPresenter
    {
        [SerializeField, ReadOnly]
        private MenuPanelView _view;

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();

            _view ??= GetComponent<MenuPanelView>();
        }
#endif
        
        public override void Initialize()
        {
            _view.Initialize();
            base.Initialize();
        }

        protected override void Bind()
        {
            base.Bind();

            _view.StartButton.OnClickAsObservable
                .SubscribeAwait(async (_, ct) =>
                {
                    await TitleManager.Instance.LoadTitleAsync();
                    WebsocketManager.Instance.ConnectWebSocket();
                    SceneManager.Instance.LoadSceneAsync(SceneType.GameScene).Forget();
                }).RegisterTo(destroyCancellationToken);
            
            _view.HowToPlayButton.OnClickAsObservable
                .Subscribe(_=>
                {
                    ChapterManager.Instance.OpenAndWaitCloseAsync<HowToPlayChapterPanelPresenter>(destroyCancellationToken).Forget();
                }).RegisterTo(destroyCancellationToken);
            
            _view.OptionButton.OnClickAsObservable
                .Subscribe(_=>
                {
                    ChapterManager.Instance.OpenAndWaitCloseAsync<OptionChapterPanelPresenter>(destroyCancellationToken).Forget();
                }).RegisterTo(destroyCancellationToken);
            
            _view.CreditButton.OnClickAsObservable
                .Subscribe(_=>
                {
                    ChapterManager.Instance.OpenAndWaitCloseAsync<CreditChapterPanelPresenter>(destroyCancellationToken).Forget();
                }).RegisterTo(destroyCancellationToken);
        }
    }
}