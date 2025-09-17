using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;

namespace Infra.Scene
{
    /// <summary>
    /// シーンを自動でEnum化するツール
    /// </summary>
    public static class SceneEnumMaker
    {
        /// <summary>
        /// Enum名
        /// </summary>
        private const string EnumName = "SceneType";
        /// <summary>
        /// Enumの名前空間
        /// </summary>
        private const string Namespace = "Common.Scene";
        /// <summary>
        /// Enumファイルのパス
        /// </summary>
        private static readonly string OutputPath = Path.Join(MainFolders.Script.AssetPath, "0_Common", "Scene");

        /// <summary>
        /// Noneのシーンパラメータ名
        /// </summary>
        private const string NONE_SCENE_PARAMETER = "None";

        /// <summary>
        /// シーン検索のパターン
        /// </summary>
        private const string SCENE_PATTERN = "*.unity";

        /// <summary>
        /// シーン名のEnumを生成する
        /// </summary>
        [MenuItem("Tools/Generate Scene Enum")]
        static void GenerateEnumFromScenes()
        {
            if (!Directory.Exists(MainFolders.Scenes.AssetPath)) return;

            // .unityファイル名（拡張子除く）を取得
            var sceneNames = Directory
                .GetFiles(MainFolders.Scenes.AssetPath, SCENE_PATTERN, SearchOption.AllDirectories)
                .Select(Path.GetFileNameWithoutExtension)
                .Distinct()
                .OrderBy(name => name)
                .ToList();

            if (sceneNames.Count == 0)
            {
                Debug.LogWarning("[SceneEnumMaker] シーンが見つかりませんでした。");
                return;
            }

            sceneNames.Insert(0, NONE_SCENE_PARAMETER);

            // 既存のEnumParameterを取得
            string jsonPath = JsonUtil.GetJsonFilePath(EnumName, OutputPath);
            var preParameters = JsonUtil.ReadJsonFile<EnumParameter[]>(jsonPath);

            // 差分がないかチェック
            if (preParameters != null)
            {
                var preSet = new HashSet<string>(preParameters.Select(p => p.Name));
                var currentSet = new HashSet<string>(sceneNames);

                if (preSet.SetEquals(currentSet))
                {
                    Debug.Log($"[SceneEnumMaker] Enum '{EnumName}' は変更なしのためスキップしました。");
                    return;
                }
            }

            EnumUtil.CreateEnum(sceneNames.ToArray(), Namespace, EnumName, OutputPath);
            Debug.Log($"[SceneEnumMaker] Enum '{EnumName}' を自動生成しました。");
        }
    }
}