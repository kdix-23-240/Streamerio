using UnityEngine;

[CreateAssetMenu(fileName = "MiddleEnemyScriptableObject", menuName = "SO/InGame/Enemy/MiddleEnemyScriptableObject")]
public class MiddleEnemyScriptableObject : ScriptableObject
{
    [Header("Middle Enemy Setting")]
    public GameObject[] Enemys;
}