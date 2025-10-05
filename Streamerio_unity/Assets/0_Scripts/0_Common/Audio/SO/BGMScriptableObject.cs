using UnityEngine;

namespace Common.Audio
{
    /// <summary>
    /// BGM 用のデータを管理する ScriptableObject。  
    /// <para>
    /// - <see cref="MusicScriptableObjectBase{TKey}"/>（BGMType）を継承  
    /// - BGMType と AudioClip の対応を保持  
    /// - AudioManager から参照され、BGM 再生に使用される
    /// </para>
    /// </summary>
    [CreateAssetMenu(fileName = "BGM", menuName = "SO/Audio/Music/BGM")]
    public class BGMScriptableObject : MusicScriptableObjectBase<BGMType> { }
}