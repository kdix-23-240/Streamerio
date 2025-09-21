using Common.Audio;
using Cysharp.Threading.Tasks;
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
        else
        {
            AudioManager.Instance.PlayAsync(SEType.どん_効果音,destroyCancellationToken).Forget();
        }
    }

    protected virtual void Die()
    {
        AudioManager.Instance.PlayAsync(SEType.敵のダウン,destroyCancellationToken).Forget();
        Destroy(gameObject);
    }
}