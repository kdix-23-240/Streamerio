using System;
using UnityEngine;

namespace Common.Audio
{
    /// <summary>
    /// 曲データを管理する ScriptableObject の基底クラス。
    /// - Enum をキーにして AudioClip を自動登録する
    /// - BGM や SE 用の ScriptableObject に継承して利用する
    /// - AutoSetDataScriptableObject を基盤としているため、
    ///   インスペクタ上で Enum 値ごとに AudioClip を簡単に設定可能
    /// </summary>
    /// <typeparam name="TKey">曲を識別する Enum 型 (例: BGMType, SEType)</typeparam>
    public class MusicScriptableObjectBase<TKey> 
        : AutoSetDataScriptableObject<TKey, AudioClip>
        where TKey : Enum
    {
        
    }
}