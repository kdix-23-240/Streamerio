using UnityEngine;

[CreateAssetMenu(fileName = "WeakEnemyScriptableObject", menuName = "SO/InGame/Enemy/WeakEnemyScriptableObject")]
public class WeakEnemyScriptableObject : ScriptableObject
{
    [Header("Weak Enemy Setting")]
    public GameObject[] Enemys;
}