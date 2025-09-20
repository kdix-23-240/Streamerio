using UnityEngine;

[CreateAssetMenu(fileName = "WeakEnemyScriptableObject", menuName = "WeakEnemyScriptableObject")]
public class WeakEnemyScriptableObject : ScriptableObject
{
    [Header("Weak Enemy Setting")]
    public GameObject[] Enemys;
}