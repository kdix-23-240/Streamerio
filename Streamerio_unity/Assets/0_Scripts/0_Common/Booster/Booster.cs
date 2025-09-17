using Common.Audio;
using Common.Scene;
using Common.UI.Guard;
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
            ClickGuard.Instance.Initialize();
            AudioManager.Instance.Initialize();

            SceneManager.Instance.LoadSceneAsync(SceneType.Title).Forget();
        }
    }
}