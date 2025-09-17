namespace Infra
{
    /// <summary>
    /// テンプレートフォルダ群
    /// </summary>
    public interface IFolderTemplate
    {
        /// <summary>
        /// 全フォルダを配列で取得
        /// </summary>
        FolderData[] GetAll();
    }
    
    /// <summary>
    /// Assets直下のフォルダ群
    /// </summary>
    public class MainFolders : IFolderTemplate
    {
        public static readonly FolderData Script = new("0_Scripts");
        public static readonly FolderData Prefabs = new("1_Prefabs");
        public static readonly FolderData ScriptableObjects = new("2_ScriptableObjects");
        public static readonly FolderData TwoD = new("3_2D");
        public static readonly FolderData Musics = new("4_Musics");
        public static readonly FolderData Scenes = new("5_Scenes");
        public static readonly FolderData Materials = new("6_Materials");
        public static readonly FolderData Animations = new("7_Animations");
        public static readonly FolderData Editor = new("Editor");
        
        public FolderData[] GetAll()
        {
            return new[]
            {
                Script,
                Prefabs,
                ScriptableObjects,
                TwoD,
                Musics,
                Scenes,
                Materials,
                Animations,
                Editor,
            };
        }
    }

    /// <summary>
    /// スクリプトフォルダのフォルダ名
    /// </summary>
    public class ScriptFolders : IFolderTemplate
    {
        public static readonly FolderData Common = new("0_Common", MainFolders.Script);
        public static readonly FolderData InGame = new("1_InGame", MainFolders.Script);
        public static readonly FolderData OutGame = new("2_OutGame", MainFolders.Script);

        public FolderData[] GetAll()
        {
            return new[]
            {
                Common,
                InGame,
                OutGame,
            };
        }
    }
    
    /// <summary>
    /// 音楽フォルダのフォルダ名
    /// </summary>
    public class MusicFolders : IFolderTemplate
    {
        public static readonly FolderData BGM = new("BGM", MainFolders.Musics);
        public static readonly FolderData SE = new("SE", MainFolders.Musics);

        public FolderData[] GetAll()
        {
            return new[]
            {
                BGM,
                SE,
            };
        }
    }
}