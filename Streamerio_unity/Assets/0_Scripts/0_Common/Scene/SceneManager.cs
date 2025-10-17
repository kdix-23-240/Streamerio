using Common.Save;
using Common.UI.Loading;
using Cysharp.Threading.Tasks;
using UnityEngine.SceneManagement;

namespace Common.Scene
{
    public class SceneManager : SingletonBase<SceneManager>
    {
        private SceneType _currentScene = SceneType.None;
        
        private bool _isReloaded = false;
        /// <summary>
        /// リロードしたか
        /// </summary>
        public bool IsReloaded => _isReloaded;


        /// <summary>
        /// シーンをロードする(前のシーンがアンロード)
        /// </summary>
        /// <param name="sceneType"></param>
        public async UniTask LoadSceneAsync(SceneType sceneType)
        {
            if (_currentScene == sceneType)
            {
                // 既に同じシーンがロードされている場合は何もしない
                return;
            }
            
            _isReloaded=false;
            //LoadingScreenPresenter.Instance.Show();

            if(_currentScene != SceneType.None)
            {
                // 現在のシーンをアンロード
                await UnityEngine.SceneManagement.SceneManager
                    .UnloadSceneAsync(_currentScene.ToString())
                    .ToUniTask(cancellationToken: destroyCancellationToken);
            }

            _currentScene = sceneType;
            await UnityEngine.SceneManagement.SceneManager
                .LoadSceneAsync(sceneType.ToString(), LoadSceneMode.Additive)
                .ToUniTask(cancellationToken: destroyCancellationToken);
        }

        /// <summary>
        /// リロード
        /// </summary>
        public async UniTask ReloadSceneAsync()
        {
            _isReloaded=true;
            //LoadingScreenPresenter.Instance.Show();

            await UnityEngine.SceneManagement.SceneManager
                .UnloadSceneAsync(_currentScene.ToString())
                .ToUniTask(cancellationToken: destroyCancellationToken);

            await UnityEngine.SceneManagement.SceneManager
                .LoadSceneAsync(_currentScene.ToString(), LoadSceneMode.Additive)
                .ToUniTask(cancellationToken: destroyCancellationToken);
        }
    }
}
