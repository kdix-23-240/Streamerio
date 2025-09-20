using UnityEngine;

[CreateAssetMenu(fileName = "MiddleEnemyScriptableObject", menuName = "MiddleEnemyScriptableObject")]
public class MiddleEnemyScriptableObject : ScriptableObject
{
    [Header("Middle Enemy Setting")]
    public GameObject[] Enemys;
}