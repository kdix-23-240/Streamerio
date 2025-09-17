using System;
using UnityEngine;

namespace Common.Audio
{
    /// <summary>
    /// 曲を自動設定するスクリプタブルオブジェクト
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    public class MusicScriptableObjectBase<TKey>: AutoSetDataScriptableObject<TKey, AudioClip>
        where TKey : Enum
    {
        
    }
}