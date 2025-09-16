using UnityEditor;

namespace Infra.Tool
{
    public class CreateTemplateFolder
    {
        [MenuItem("Tools/Create Template Folders")]
        static void Create()
        {
            CreateFolders(new MainFolders().GetAll());
            CreateFolders(new ScriptFolders().GetAll());
            CreateFolders(new MusicFolders().GetAll());
        }
    
        /// <summary>
        /// フォルダを作成
        /// </summary>
        /// <param name="folders"></param>
        private static void CreateFolders(FolderData[] folders)
        {
            foreach (var folder in folders)
            {
                FolderUtil.CreateUniqueFolder(folder);
            }
            
            AssetDatabase.Refresh();
        }
    }
}