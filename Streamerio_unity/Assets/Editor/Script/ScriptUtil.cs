using System.IO;
using Cysharp.Text;
using UnityEditor;

namespace Infra
{
    public static class ScriptUtil
    {
        /// <summary>
        /// スクリプトの拡張子
        /// </summary>
        public static readonly Extention Extention = new("cs");

        /// <summary>
        /// フィルターのフォーマット
        /// <para>0: ファイル名</para>
        /// </summary>
        private const string SearchFilterFormat = "{0} t:Script";
        /// <summary>
        /// フィルターを取得
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        private static string GetSearchFilter(string fileName)
            => ZString.Format(SearchFilterFormat, fileName);

        /// <summary>
        /// スクリプトを探す
        /// </summary>
        /// <param name="fileName">ファイル名</param>
        /// <returns>見つからなかった場合は、空文字列</returns>
        public static string FindScriptPath(string fileName)
        {
            string[] guids = AssetDatabase.FindAssets(GetSearchFilter(fileName));
            foreach (var guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                string findFileName = Path.GetFileNameWithoutExtension(path);

                if (findFileName == fileName)
                {
                    return path;
                }
            }
            
            Debug.LogWarning($"{fileName}が見つかりませんでした");
            return string.Empty;
        }
        
        /// <summary>
        /// スクリプトを書き込んで保存する(既にファイルがある場合は上書き)
        /// </summary>
        /// <param name="script"></param>
        public static void SaveScript(Script script)
        {
            File.WriteAllText(script.FilePath, script.Code.Contents);
            Debug.Log("Create Script: " + script.FilePath);
            
            AssetDatabase.Refresh();
        }
        
        /// <summary>
        /// スクリプトを書き込んで保存する(既にファイルがある場合は何もしない)
        /// </summary>
        /// <param name="script"></param>
        public static void SaveScriptIfNotExists(Script script)
        {
            if (File.Exists(script.FilePath))
            {
                Debug.Log("Already Create Script: " + script.FilePath);
                return;
            }
            
            SaveScript(script);
        }

        /// <summary>
        /// フォルダとそのフォルダ内のスクリプトをすべて削除
        /// </summary>
        /// <param name="directoryPath"></param>
        public static void DeleteScripts(string directoryPath)
        {
            if (!Directory.Exists(directoryPath))
            {
                Debug.Log("Directory is not Exist: " + directoryPath);
                return;
            }
            
            /* スクリプト削除 */
            var scriptPaths = Directory.GetFiles(directoryPath, Extention.ToPattern(), SearchOption.AllDirectories);
            foreach (var scriptPath in scriptPaths)
            {
                string unityPath = scriptPath.Replace("\\", "/");
                if (AssetDatabase.DeleteAsset(unityPath))
                {
                    Debug.Log($"Delete Success: {unityPath}");
                }
                else
                {
                    Debug.LogError($"Delete Filed: {unityPath}");
                }
            }
            
            /* フォルダ削除 */
            if (AssetDatabase.DeleteAsset(directoryPath))
            {
                Debug.Log($"Delete Success: {directoryPath}");
            }
            else
            {
                Debug.LogError($"Delete Filed: {directoryPath}");
            }

            AssetDatabase.Refresh();
        }
    }
}