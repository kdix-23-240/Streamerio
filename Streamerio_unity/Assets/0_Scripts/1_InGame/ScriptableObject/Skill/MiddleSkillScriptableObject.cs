using UnityEngine;

[CreateAssetMenu(fileName = "MiddleSkillScriptableObject", menuName = "MiddleSkillScriptableObject")]
public class MiddleSkillScriptableObject : ScriptableObject
{
    [Header("Middle Skill Setting")]
    public GameObject[] Skills;
}