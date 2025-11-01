using UnityEngine;

namespace Common.Audio
{
    /// <summary>
    /// BGM データを管理する ScriptableObject。
    /// - MusicScriptableObjectBase&lt;BGMType&gt; を継承
    /// - BGMType (列挙型) と AudioClip の対応関係を保持
    /// - AudioManager から参照され、BGM の再生に利用される
    /// </summary>
    [CreateAssetMenu(fileName = "BGM", menuName = "SO/Audio/Music/BGM")]
    public class BGMScriptableObject : MusicScriptableObjectBase<BGMType>
    {
        // 基底クラスで必要な機能をすべて提供しているため、
        // ここでは型指定のみを行う。
    }
}