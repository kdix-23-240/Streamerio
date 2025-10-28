using System.Threading;
using Common.Scene;
using Common.UI.Loading;
using Cysharp.Threading.Tasks;
using VContainer;

namespace Common.State
{
    public class RestartState: IState
    {
        private readonly ILoadingScreen _loadingScreen;
        private readonly ISceneManager _sceneManager;
        
        [Inject]
        public RestartState(ILoadingScreen loadingScreen, ISceneManager sceneManager)
        {
            _loadingScreen = loadingScreen;
            _sceneManager = sceneManager;
        }
        
        public async UniTask EnterAsync(CancellationToken ct)
        {
            await _loadingScreen.ShowAsync(ct);
            _sceneManager.UpdateRestartFlag(true);
            await _sceneManager.LoadSceneAsync(SceneType.GameScene);
        }
        
        public async UniTask ExitAsync(CancellationToken ct)
        {
            await UniTask.WaitForEndOfFrame(cancellationToken: ct);
        }
    }
}