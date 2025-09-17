using UnityEditor;

namespace Common.Audio.Editor
{
    /// <summary>
    /// BGMを自動設定するスクリプタブルオブジェクトのエディタ
    /// </summary>
    [CustomEditor(typeof(BGMScriptableObject))]
    public class BGMScriptableObjectEditor: MusicScriptableObjectEditor<BGMType, BGMScriptableObject>
    {
        
    }
}