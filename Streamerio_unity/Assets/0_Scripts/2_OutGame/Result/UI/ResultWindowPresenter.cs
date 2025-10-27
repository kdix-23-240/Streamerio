using Common;
using Common.Scene;
using Common.UI.Display.Overlay;
using Common.UI.Display.Window;
using Common.UI.Loading;
using Cysharp.Threading.Tasks;
using R3;

namespace OutGame.Result.UI
{
    public interface IResultWindow : IWindow, IAttachable<ResultWindowContext>
    {
    }
    
    /// <summary>
    /// ゲームクリア時のオーバーレイ Presenter。
    /// - 背景クリックでタイトルシーンへ遷移
    /// - クリック誘導テキストのアニメーションを制御
    /// </summary>
    public class ResultWindowPresenter : WindowPresenterBase<IResultWindowView, ResultWindowContext>, IResultWindow
    {
        private ILoadingScreen _loadingScreen;
        private ISceneManager _sceneManager;

        protected override void Bind()
        {
            base.Bind();
            View.Background.OnClickAsObservable
                .Subscribe(_ => { CloseEvent(); })
                .RegisterTo(GetCt());
        }

        protected override void AttachContext(ResultWindowContext context)
        {
            base.AttachContext(context);

            _loadingScreen = context.LoadingScreen;
            _sceneManager = context.SceneManager;
        }
        
        protected override async void CloseEvent()
        {
            await _loadingScreen.ShowAsync(GetCt());
            _sceneManager.LoadSceneAsync(SceneType.Title).Forget();
        }
    }

    public class ResultWindowContext : WindowContext<IResultWindowView>
    {
        public ILoadingScreen LoadingScreen;
        public ISceneManager SceneManager;
    }
}