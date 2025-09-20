using UnityEngine;

[CreateAssetMenu(fileName = "WeakSkillScriptableObject", menuName = "WeakSkillScriptableObject")]
public class WeakSkillScriptableObject : ScriptableObject
{
    [Header("Weak Skill Setting")]
    public GameObject[] Skills;
}