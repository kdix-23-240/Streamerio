using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Common.Scene
{
    public class SceneManager : SingletonBase<SceneManager>
    {
        private SceneType _currentScene = SceneType.None;

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

            //await StateManager.Instance.ChangeStateAsync(LoadingState.Instance);

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
            //await StateManager.Instance.ChangeStateAsync(LoadingState.Instance);

            await UnityEngine.SceneManagement.SceneManager
                .UnloadSceneAsync(_currentScene.ToString())
                .ToUniTask(cancellationToken: destroyCancellationToken);

            await UnityEngine.SceneManagement.SceneManager
                .LoadSceneAsync(_currentScene.ToString(), LoadSceneMode.Additive)
                .ToUniTask(cancellationToken: destroyCancellationToken);
        }
    }
}
