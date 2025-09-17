using System;
using Cysharp.Text;
using UnityEditor;
using UnityEngine;

namespace Infra
{
    /// <summary>
    /// よく使うPrefab操作
    /// </summary>
    public static class PrefabUtil
    {
        public static readonly Extention Extention = new Extention("prefab");
        
        /// <summary>
        /// 指定されたパスのオブジェクトのプレファブを生成
        /// </summary>
        /// <param name="sourcePath">生成するオブジェクトのパス</param>
        /// <param name="savePath">生成したプレファブを保存するパス</param>
        public static void Create(string sourcePath, string savePath)
        {
            try
            {
                var obj = AssetDatabase.LoadAssetAtPath<GameObject>(sourcePath);
                var tmpInstance = GameObject.Instantiate(obj);
                PrefabUtility.SaveAsPrefabAsset(tmpInstance, savePath);
                GameObject.DestroyImmediate(tmpInstance);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }
    }
}