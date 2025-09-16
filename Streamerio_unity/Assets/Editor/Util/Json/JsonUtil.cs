using System.IO;
using Cysharp.Text;
using Unity.Plastic.Newtonsoft.Json;
using UnityEngine;

namespace Infra
{
    public static class JsonUtil
    {
        private static readonly Extention _extention = new Extention("json");
        
        /// <summary>
        /// Jsonファイルを読み込む
        /// </summary>
        /// <param name="path"></param>
        /// <returns> 読み込めない場合は、デフォルト値 </returns>
        public static T ReadJsonFile<T>(string path)
        {
            if (!File.Exists(path))
            {
                return default;
            }
            
            string json = File.ReadAllText(path);
            return JsonConvert.DeserializeObject<T>(json);
        }

        /// <summary>
        /// Jsonファイルを保存する
        /// </summary>
        /// <param name="parameters"></param>
        /// <param name="path"></param>
        public static void SaveJsonFile<T>(T parameters, string path)
        {
            string json = JsonConvert.SerializeObject(parameters, Formatting.Indented);
            File.WriteAllText(path, json);
        }

        /// <summary>
        /// Jsonファイルのパスを取得
        /// </summary>
        /// <param name="fileName">ファイル名</param>
        /// <param name="root"> Assetからのパス </param>
        /// <returns></returns>
        public static string GetJsonFilePath(string fileName, string root)
        {
            string jsonFileName = _extention.ToFileName(fileName);
            return Path.Combine(Application.dataPath.Replace(FolderUtil.AssetsFolderName, string.Empty), root, jsonFileName);
        }

        /// <summary>
        /// Jsonファイルを探索
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns>見つからない場合は、空文字</returns>
        public static string FindJsonFilePath(string fileName)
        {
            var files = Directory.GetFiles(MainFolders.Script.FullPath, _extention.ToFileName(fileName),
                SearchOption.AllDirectories);
            
            return files.Length > 0 ? files[0] : string.Empty;
        }
    }
}