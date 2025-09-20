using UnityEngine;

[CreateAssetMenu(fileName = "StrongEnemyScriptableObject", menuName = "StrongEnemyScriptableObject")]
public class StrongEnemyScriptableObject : ScriptableObject
{
    [Header("Strong Enemy Setting")]
    public GameObject[] Enemys;
}