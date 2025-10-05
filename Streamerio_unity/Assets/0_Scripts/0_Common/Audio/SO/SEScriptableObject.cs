using UnityEngine;

namespace Common.Audio
{
    /// <summary>
    /// SE（効果音）用のデータを管理する ScriptableObject。  
    /// <para>
    /// - <see cref="MusicScriptableObjectBase{TKey}"/>（SEType）を継承  
    /// - SEType と AudioClip の対応を保持  
    /// - インスペクタ上で SEType ごとに AudioClip を設定可能
    /// </para>
    /// </summary>
    [CreateAssetMenu(fileName = "SE", menuName = "SO/Audio/Music/SE")]
    public class SEScriptableObject : MusicScriptableObjectBase<SEType> { }
}