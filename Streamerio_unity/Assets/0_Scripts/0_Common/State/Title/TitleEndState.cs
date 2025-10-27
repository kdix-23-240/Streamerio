using System.Threading;
using Common.Scene;
using Common.UI.Animation;
using Common.UI.Loading;
using Cysharp.Threading.Tasks;
using VContainer;

namespace Common.State
{
    public class TitleEndState: IState
    {
        private readonly IUIAnimation _titleBackgroundAnimation;
        private readonly ILoadingScreen _loadingScreen;
        private readonly ISceneManager _sceneManager;
        
        [Inject]
        public TitleEndState(
            [Key(AnimationType.TitleBackground)] IUIAnimation titleBackgroundAnimation,
            ILoadingScreen loadingScreen,
            ISceneManager sceneManager)
        {
            _titleBackgroundAnimation = titleBackgroundAnimation;
            _loadingScreen = loadingScreen;
            _sceneManager = sceneManager;
        }
        
        public async UniTask EnterAsync(CancellationToken ct)
        {
            _titleBackgroundAnimation.PlayAsync(ct).Forget();
            await _loadingScreen.ShowAsync(ct);
            await _sceneManager.LoadSceneAsync(SceneType.GameScene);
        }
        
        public async UniTask ExitAsync(CancellationToken ct)
        {
            await UniTask.WaitForEndOfFrame(cancellationToken: ct);
        }
    }
}