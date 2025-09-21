using UnityEngine;

public class EnemyHpManager : MonoBehaviour
{
    [SerializeField] private int health = 50;
    public int CurrentHealth => health;
    public bool IsDead => health <= 0;

    public void TakeDamage(int amount)
    {
        if (IsDead) return;
        health -= amount;
        if (health <= 0)
        {
            health = 0;
            Die();
        }
    }

    protected virtual void Die()
    {
        Destroy(gameObject);
    }
}