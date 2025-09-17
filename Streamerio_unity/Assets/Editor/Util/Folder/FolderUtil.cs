using UnityEditor;

namespace Infra
{
    public static class FolderUtil
    {
        public const string AssetsFolderName = "Assets";

        /// <summary>
        /// フォルダを作成
        /// </summary>
        /// <param name="folderData"></param>
        /// <returns>既にある場合はfalse</returns>
        public static bool CreateUniqueFolder(FolderData folderData)
        {
            if (!AssetDatabase.IsValidFolder(folderData.AssetPath))
            {
                var result = AssetDatabase.CreateFolder(folderData.ParentPath, folderData.Name);

                if (!string.IsNullOrEmpty(result))
                {
                    Debug.Log($"Success to create folder: {folderData.AssetPath}");
                    return true;   
                }
                
                Debug.LogError($"Failed to create folder: {folderData.AssetPath}");
            }
            
            Debug.LogWarning($"Already exists folder: {folderData.AssetPath}");
            return false;
        }
    }
}