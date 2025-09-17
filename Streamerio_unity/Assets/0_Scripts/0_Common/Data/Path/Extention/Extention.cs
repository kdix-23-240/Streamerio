using Cysharp.Text;
using System;
using UnityEngine;

namespace Infra
{
    /// <summary>
    /// 拡張子
    /// </summary>
    [Serializable]
    public class Extention
    {
        private const string _dot = ".";
        
        [SerializeField, Header("値(ex: png, mp3)")]
        private string _value;

        public Extention(string value)
        {
            _value = value;
        }
        
        public override string ToString()
        {
            return ZString.Concat(_dot, _value);
        }

        /// <summary>
        /// 拡張子付きのファイル名に変換
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public string ToFileName(string fileName)
        {
            return ZString.Concat(fileName, _dot, _value);
        }

        /// <summary>
        /// パターン検索のフィルターに変換
        /// </summary>
        /// <param name="fileName">ファイル名を指定する場合は設定(デフォルトは、ワイルドカード)</param>
        /// <returns></returns>
        public string ToPattern(string fileName = "*")
        {
            return ZString.Concat(fileName, _dot, _value);
        }
    }
}