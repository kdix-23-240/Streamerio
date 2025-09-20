using UnityEngine;

[CreateAssetMenu(fileName = "WeakSkillScriptableObject", menuName = "SO/InGame/Skill/WeakSkillScriptableObject")]
public class WeakSkillScriptableObject : ScriptableObject
{
    [Header("Weak Skill Setting")]
    public GameObject[] Skills;
}