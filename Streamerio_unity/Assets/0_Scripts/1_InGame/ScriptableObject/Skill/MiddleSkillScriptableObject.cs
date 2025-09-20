using UnityEngine;

[CreateAssetMenu(fileName = "MiddleSkillScriptableObject", menuName = "SO/InGame/Skill/MiddleSkillScriptableObject")]
public class MiddleSkillScriptableObject : ScriptableObject
{
    [Header("Middle Skill Setting")]
    public GameObject[] Skills;
}