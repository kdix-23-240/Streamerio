using UnityEngine;

[CreateAssetMenu(fileName = "StrongSkillScriptableObject", menuName = "SO/InGame/Skill/StrongSkillScriptableObject")]
public class StrongSkillScriptableObject : ScriptableObject
{
    [Header("Strong Skill Setting")]
    public GameObject[] Skills;
}