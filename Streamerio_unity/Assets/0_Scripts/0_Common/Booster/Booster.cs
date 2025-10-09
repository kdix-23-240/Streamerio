using Common.Audio;
using Common.Scene;
using Common.UI.Loading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Common.Booster
{
    /// <summary>
    /// ゲームの初期設定
    /// </summary>
    public class Booster: MonoBehaviour
    {
        private void Start()
        {
            LoadingScreenPresenter.Instance.Initialize();
            LoadingScreenPresenter.Instance.Show();
            AudioManager.Instance.Initialize();
            WebsocketManager.Instance.HealthCheck();
            SceneManager.Instance.LoadSceneAsync(SceneType.Title).Forget();
        }
    }
}