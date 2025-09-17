using UnityEditor;
using UnityEditor.SceneManagement;

namespace Infra.Scene
{
    [InitializeOnLoad]
    public static class ScenePlayer
    {
        static ScenePlayer()
        {
            // 再生ボタンが押されたときは、現在開いているシーンを再生
            EditorApplication.playModeStateChanged += 
                (state) =>
                {
                    if (state == PlayModeStateChange.EnteredPlayMode)
                    {
                        EditorSceneManager.playModeStartScene = null;
                    }
                };
        }
    
        /// <summary>
        /// ビルド設定の最初のシーンを再生
        /// </summary>
        [MenuItem("Tools/Play Game")]
        public static void PlayFirstScene()
        {
            if (EditorBuildSettings.scenes.Length == 0)
            {
                Debug.LogError("Scenes in Build settings is empty");
                return;
            }
        
            Play(EditorBuildSettings.scenes[0].path);
        }
    
        /// <summary>
        /// 指定したパスにあるシーンの再生
        /// </summary>
        /// <param name="path">シーンのパス</param>
        private static void Play(string path)
        {
            // シーンをロード
            var scene = AssetDatabase.LoadAssetAtPath<SceneAsset>(path);

            if (scene == null)
            {
                Debug.LogError(path + " Scene not found");
                return;
            }
        
            // シーンを再生
            EditorSceneManager.playModeStartScene = scene;
            EditorApplication.isPlaying = true;
        }
    }

}