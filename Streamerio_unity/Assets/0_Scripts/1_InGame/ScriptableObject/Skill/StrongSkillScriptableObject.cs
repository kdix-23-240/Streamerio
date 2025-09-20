using UnityEngine;

[CreateAssetMenu(fileName = "StrongSkillScriptableObject", menuName = "StrongSkillScriptableObject")]
public class StrongSkillScriptableObject : ScriptableObject
{
    [Header("Strong Skill Setting")]
    public GameObject[] Skills;
}