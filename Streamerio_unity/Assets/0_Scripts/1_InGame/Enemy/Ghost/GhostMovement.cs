using UnityEngine;

public class GhostMovement : MonoBehaviour
{
    [Header("移動設定")]
    [SerializeField] private float speed = 1.5f;
    [SerializeField] private float attackCooldown = 0.7f;
    
    private Transform _player;
    private EnemyAttackManager _attackManager;
    private float _lastAttackTime = -999f;
    
    void Start()
    {
        // プレイヤーを探す
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            _player = playerObj.transform;
        }
        
        _attackManager = GetComponent<EnemyAttackManager>();
        
        // 重力を無効化
        var rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.gravityScale = 0f;
        }
    }
    
    void Update()
    {
        if (_player == null) return;
        
        MoveTowardsPlayer();
    }
    
    private void MoveTowardsPlayer()
    {
        // プレイヤーの方向を計算
        Vector2 direction = (_player.position - transform.position).normalized;
        
        // プレイヤーに向かって移動
        transform.position += (Vector3)(direction * speed * Time.deltaTime);
        
        // スプライトの向きを調整
        if (direction.x < 0)
        {
            transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        }
        else
        {
            transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        }
    }
    
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            AttackPlayer();
        }
    }
    
    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            AttackPlayer();
        }
    }
    
    private void AttackPlayer()
    {
        if (Time.time - _lastAttackTime < attackCooldown) return;
        if (_attackManager == null) return;
        
        Debug.Log($"Ghost attacked player for {_attackManager.CurrentDamage} damage!");
        _lastAttackTime = Time.time;
    }
}