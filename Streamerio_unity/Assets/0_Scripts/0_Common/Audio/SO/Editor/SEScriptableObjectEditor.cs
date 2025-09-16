using UnityEditor;

namespace Common.Audio.Editor
{
    /// <summary>
    /// SEを自動設定するスクリプタブルオブジェクトのエディタ
    /// </summary>
    [CustomEditor(typeof(SEScriptableObject))]
    public class SEScriptableObjectEditor: MusicScriptableObjectEditor<SEType, SEScriptableObject>
    {
        
    }
}