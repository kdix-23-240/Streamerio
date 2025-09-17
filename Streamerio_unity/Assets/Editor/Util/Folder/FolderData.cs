using System.IO;
using UnityEngine;

namespace Infra
{
    /// <summary>
    /// フォルダの情報
    /// </summary>
    public class FolderData
    {
        /// <summary>
        /// フォルダ名
        /// </summary>
        public readonly string Name;

        /// <summary>
        /// 絶対バス(ドライブから)
        /// </summary>
        public readonly string FullPath;
        /// <summary>
        /// Assetsからのフォルダパス(Assetsから)
        /// </summary>
        public readonly string AssetPath;
        /// <summary>
        /// Assetsより下のフォルダパス(Assetsを除く)
        /// </summary>
        public readonly string RelativePath;

        /// <summary>
        /// 親パス(Assetsから)
        /// </summary>
        public readonly string ParentPath;
        
        /// <summary>
        /// フォルダ情報
        /// </summary>
        /// <param name="name">フォルダ名</param>
        /// <param name="parent">Assetsより下のルートパス(Assets除く)</param>
        public FolderData(string name, string parent = "")
        {
            // 名前
            Name = name;
            
            // パス
            RelativePath = Path.Combine(parent, Name);
            FullPath = Path.Combine(Application.dataPath, RelativePath);
            AssetPath = Path.Combine(FolderUtil.AssetsFolderName, RelativePath);
            ParentPath = Path.Combine(FolderUtil.AssetsFolderName, parent);
        }
        
        public FolderData(string name, FolderData parent)
        {
            // 名前
            Name = name;
            
            // パス
            RelativePath = Path.Combine(parent.RelativePath, Name);
            FullPath = Path.Combine(Application.dataPath, RelativePath);
            AssetPath = Path.Combine(FolderUtil.AssetsFolderName, RelativePath);
            ParentPath = parent.AssetPath;
        }
    }
}