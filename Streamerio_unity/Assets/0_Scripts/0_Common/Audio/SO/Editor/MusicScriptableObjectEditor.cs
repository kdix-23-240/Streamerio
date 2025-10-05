using Common.Editor;
using System;
using UnityEngine;

namespace Common.Audio.Editor
{
    /// <summary>
    /// 曲を自動設定するスクリプタブルオブジェクトのエディタ拡張
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TSO"></typeparam>
    public class MusicScriptableObjectEditor<TKey, TSO>: AutoSetDataScriptableObjectEditor<TKey, MusicData, AudioClip, TSO>
        where TKey: Enum
        where TSO: MusicScriptableObjectBase<TKey>
    {
        protected override void OnEnable()
        {
            base.OnEnable();
            IsSetNone = true;
        }
        
        protected override MusicData CreateValue(AudioClip file)
        {
            return new MusicData()
            {
                Clip = file,
                Capacity = Target.DefaultCapacity
            };
        }
    }
}