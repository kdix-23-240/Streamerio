using Common.Audio;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class EnemyHpManager : MonoBehaviour
{
    private int _health;
    public int CurrentHealth => _health;
    public bool IsDead => _health <= 0;

    public void Initialize(int health)
    {
        _health = health;
    }

    public void TakeDamage(int amount)
    {
        if (IsDead) return;
        _health -= amount;
        if (_health <= 0)
        {
            _health = 0;
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