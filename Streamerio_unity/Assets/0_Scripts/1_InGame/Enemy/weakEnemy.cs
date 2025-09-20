using UnityEngine;

public class WeakEnemy : MonoBehaviour
{
    [Header("ステータス")]
    [SerializeField] private int health = 50;
    [SerializeField] private int damage = 10;
    [SerializeField] private float speed = 2f;

    [Header("ダメージ適用先 (プレイヤーHP)")]
    [SerializeField] private HpPresenter hpPresenter; // プレイヤーの HpPresenter を直接アタッチ

    [Header("当たり判定")]
    [SerializeField] private string playerTag = "Player"; // 何に当たったらダメージを与えるかの判定用
    [SerializeField] private string bulletTag = "Bullet"; // 弾のタグ
    [SerializeField] private float hitCooldown = 0.4f;     // 連続ヒット抑制

    private float _lastHitTime = -999f;

    void Awake()
    {
        if (hpPresenter == null)
        {
            Debug.LogWarning("[WeakEnemy] hpPresenter 未割当です。シーン上のプレイヤーHPオブジェクトをインスペクタでセットしてください。");
        }
    }

    void Update()
    {
        // シンプルな左移動。AI化するならここを差し替え
        transform.Translate(Vector2.left * speed * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // まず何と接触したか確認
        //Debug.Log($"Triggered by: {collision.gameObject.name}, Tag: {collision.gameObject.tag}");
        
        if (collision.gameObject.CompareTag(playerTag))
        {
            //Debug.Log("Player detected!");
            Attack();
        }
            if (collision.gameObject.CompareTag(bulletTag))
            {
                //Debug.Log("Enemy hit by bullet!");
                // 弾に当たったらダメージを受ける
                var bullet = collision.gameObject.GetComponent<NormalBullet>();
                if (bullet != null)
                {
                TakeDamage((int)bullet.Damage);
                // オブジェクトプールを使用している場合はDestroyではなくプールに戻す
                var bulletPool = FindObjectOfType<BulletPool>();
                if (bulletPool != null)
                {
                    bulletPool.ReturnBullet(bullet);
                }
                else
                {
                    Destroy(collision.gameObject);
                }
            }
        }
    }


    private void Attack()
    {
        if (hpPresenter == null) return;
        if (Time.time - _lastHitTime < hitCooldown) return;

        hpPresenter.Decrease(damage);
        //Debug.Log($"Enemy attacked! Player HP: {hpPresenter.Amount}");
        _lastHitTime = Time.time;
    }

    public void TakeDamage(int amount)
    {
        health -= amount;
        if (health <= 0) Destroy(gameObject);
    }
}