using UnityEngine;

namespace Common.Audio
{
    /// <summary>
    /// SE (効果音) を管理する ScriptableObject。
    /// - Enum(SEType) をキーにして AudioClip を自動登録する
    /// - MusicScriptableObjectBase を継承して実装
    /// - インスペクタ上で SEType ごとに AudioClip を設定して利用可能
    /// </summary>
    [CreateAssetMenu(fileName = "SE", menuName = "SO/Audio/Music/SE")]
    public class SEScriptableObject : MusicScriptableObjectBase<SEType>
    {
        // 特殊な処理は不要。基底クラスで Enum→AudioClip の辞書化を行う。
    }
}