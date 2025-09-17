using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Infra
{
    /// <summary>
    /// スライスされたスプライトをインポートする
    /// </summary>
    public static class SlicedSpriteLoader
    {
        /// <summary>
        /// テクスチャから切り出したスプライトを取得
        /// </summary>
        /// <param name="texture"></param>
        /// <returns></returns>
        public static Sprite[] Load(Texture2D texture)
        {
            string path = AssetDatabase.GetAssetPath(texture);
            return AssetDatabase.LoadAllAssetRepresentationsAtPath(path)
                .OfType<Sprite>()
                .ToArray();
        }
    }
}