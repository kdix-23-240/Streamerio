using UnityEditor;
using UnityEngine;

namespace Infra.Automation
{
    /// <summary>
    /// AudioClipインポート時の自動処理ツール
    /// </summary>
    public class MusicImporter: AssetPostprocessor
    {
        void OnPreprocessAudio()
        {
            // 曲のフォルダ以外の場合は何もしない
            if (!assetPath.Contains(MainFolders.Musics.Name))
            {
                return;
            }

            AudioImporter musicImporter = (AudioImporter)assetImporter;
            
            musicImporter.forceToMono = true; // モノラルにして容量削減

            AudioImporterSampleSettings settings = musicImporter.defaultSampleSettings;
            if (assetPath.Contains(MusicFolders.BGM.Name)) // Bgmの場合は、直接オーディオデータを流す設定
            {
                settings.loadType = AudioClipLoadType.Streaming;
            }
            else if (assetPath.Contains(MusicFolders.SE.Name)) // Seの場合は、ロード中にサウンドを解凍する設定
            {
                settings.loadType = AudioClipLoadType.DecompressOnLoad;
            }
            musicImporter.defaultSampleSettings = settings;
        }
    }
}