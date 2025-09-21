using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;

public class UltThunder : MonoBehaviour
{
    [SerializeField] private float _speed = 25f;
    [SerializeField] private float _damage = 90f;
    [SerializeField] private float _lifetime = 3f;
    [SerializeField] private int _strikeCount = 3;
    [SerializeField] private float _strikeInterval = 0.5f;
    [SerializeField] private float _strikeRange = 3f;
    [SerializeField] private float _continuousDamageInterval = 0.4f; // 持続ダメージ間隔(秒)
    [SerializeField] private float _continuousDamage = 30f; // 持続ダメージ量
    
    private int _currentStrikes = 0;
    private float _strikeTimer = 0f;
    private List<GameObject> _hitEnemies = new List<GameObject>();
    private Dictionary<GameObject, int> _enemyDamageCounters = new Dictionary<GameObject, int>();
    private int _damageIntervalFrames;
    private GameObject _player;

    void Awake()
    {
        _player = GameObject.FindWithTag("Player");
        if (_player == null)
        {
            Debug.LogError("Player object not found in the scene.");
        }

    }
    void Start()
    {
        if (_player != null)
        {
            //playerのy座標から8マス右側に生成
            transform.position = new Vector2(_player.transform.position.x + 8f, _player.transform.position.y);
        }
        // フレームベースでインターバルを計算
        _damageIntervalFrames = Mathf.RoundToInt(_continuousDamageInterval / Time.fixedDeltaTime);
        
        // 縦方向（上から下）への攻撃開始
        StartThunderStrike();
    }

    void Update()
    {
        HandleThunderStrike();
        HandleContinuousDamage(); // 持続ダメージ処理を追加
        
        if (_lifetime <= 0)
        {
            DestroySkill();
        }
        _lifetime -= Time.deltaTime;
    }

    private void HandleContinuousDamage()
    {
        // 範囲内の敵に対して持続ダメージを処理
        Collider2D[] enemies = Physics2D.OverlapBoxAll(
            transform.position, 
            new Vector2(1f, _strikeRange), 
            0f
        );

        foreach (var enemyCollider in enemies)
        {
            if (enemyCollider.CompareTag("Enemy"))
            {
                if (!_enemyDamageCounters.ContainsKey(enemyCollider.gameObject))
                {
                    // 新しい敵をカウンターに追加
                    _enemyDamageCounters[enemyCollider.gameObject] = 0;
                    
                    // 初期ダメージ
                    var enemy = enemyCollider.GetComponent<EnemyHpManager>();
                    if (enemy != null)
                    {
                        //Debug.Log($"UltThunder entered range: {enemyCollider.gameObject.name}");
                        enemy.TakeDamage((int)_damage);
                    }
                }
                else
                {
                    // 既存の敵のカウンターを更新
                    _enemyDamageCounters[enemyCollider.gameObject]++;
                    
                    // インターバルに達したら持続ダメージを与える
                    if (_enemyDamageCounters[enemyCollider.gameObject] >= _damageIntervalFrames)
                    {
                        var enemy = enemyCollider.GetComponent<EnemyHpManager>();
                        if (enemy != null)
                        {
                            //Debug.Log($"UltThunder continuous damage: {enemyCollider.gameObject.name}");
                            enemy.TakeDamage((int)_continuousDamage);
                        }
                        
                        // カウンターリセット
                        _enemyDamageCounters[enemyCollider.gameObject] = 0;
                    }
                }
            }
        }

        // 範囲から出た敵をカウンターから削除
        List<GameObject> enemiesToRemove = new List<GameObject>();
        foreach (var enemy in _enemyDamageCounters.Keys)
        {
            bool stillInRange = false;
            foreach (var enemyCollider in enemies)
            {
                if (enemyCollider.gameObject == enemy)
                {
                    stillInRange = true;
                    break;
                }
            }
            
            if (!stillInRange)
            {
                enemiesToRemove.Add(enemy);
            }
        }
        
        foreach (var enemy in enemiesToRemove)
        {
            _enemyDamageCounters.Remove(enemy);
            //Debug.Log($"UltThunder exited range: {enemy.name}");
        }
    }

    private async void StartThunderStrike()
    {
        // 雷は上から下への連続攻撃
        for (int i = 0; i < _strikeCount; i++)
        {
            PerformThunderStrike();
            await UniTask.Delay((int)(_strikeInterval * 1000));
            
            if (this == null) break; // オブジェクトが破壊された場合
        }
    }

    private void HandleThunderStrike()
    {
        // 雷エフェクトの表現（点滅など）
        _strikeTimer += Time.deltaTime;
        if (_strikeTimer > 0.1f)
        {
            var renderer = GetComponent<SpriteRenderer>();
            if (renderer != null)
            {
                renderer.enabled = !renderer.enabled;
            }
            _strikeTimer = 0f;
        }
    }

    private void PerformThunderStrike()
    {
        // 縦方向の攻撃範囲内の敵を検索
        Collider2D[] enemies = Physics2D.OverlapBoxAll(
            transform.position, 
            new Vector2(1f, _strikeRange), 
            0f
        );

        foreach (var enemyCollider in enemies)
        {
            if (enemyCollider.CompareTag("Enemy") && !_hitEnemies.Contains(enemyCollider.gameObject))
            {
                var enemy = enemyCollider.GetComponent<EnemyHpManager>();
                if (enemy != null)
                {
                    Debug.Log($"UltThunder struck: {enemyCollider.gameObject.name}");
                    enemy.TakeDamage((int)_damage);
                    _hitEnemies.Add(enemyCollider.gameObject);
                }
            }
        }
        
        _currentStrikes++;
    }
    public void DestroySkill()
    {
        Destroy(gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        // エディター上で攻撃範囲を可視化
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(transform.position, new Vector3(1f, _strikeRange, 0));
    }
}