using UnityEngine;
using UnityEngine.UI;

public class WeakEnemyButton : MonoBehaviour
{
    [SerializeField] private EnemyRandomActivator _enemyRandomActivator; // EnemyRandomActivatorの参照

    public void OnClick()
    {
        _enemyRandomActivator.ActivateWeakEnemy();
    }
}