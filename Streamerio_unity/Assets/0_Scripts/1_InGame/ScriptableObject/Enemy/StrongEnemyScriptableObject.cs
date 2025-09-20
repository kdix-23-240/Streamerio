using UnityEngine;

[CreateAssetMenu(fileName = "StrongEnemyScriptableObject", menuName = "SO/InGame/Enemy/StrongEnemyScriptableObject")]
public class StrongEnemyScriptableObject : ScriptableObject
{
    [Header("Strong Enemy Setting")]
    public GameObject[] Enemys;
}