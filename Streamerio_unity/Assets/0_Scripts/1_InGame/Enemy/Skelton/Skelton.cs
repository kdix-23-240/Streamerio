using Common.Audio;
using Cysharp.Threading.Tasks;
using UnityEngine;
using VContainer;

public class Skelton : MonoBehaviour, IAttackable, IHealth
{
    // DI経由で注入される依存関係
    private IEnemyStats _enemyStats;
    private EnemyHpManager _enemyHpManager;
    
    // 内部状態
    private float _speed;
    private float _startMoveDelay;
    private float _spawnTime;
    private bool _canMove = false;
    private Transform _player;

    // IAttackable, IHealth の実装
    public float Power => _enemyStats.Power;
    public int Health => _enemyStats.Health;

    /// <summary>
    /// VContainer によるコンストラクタインジェクション
    /// </summary>
    [Inject]
    public void Construct(IEnemyStats enemyStats, EnemyHpManager enemyHpManager)
    {
        _enemyStats = enemyStats;
        _enemyHpManager = enemyHpManager;
        
        // パラメーター初期化
        _speed = _enemyStats.Speed;
        _startMoveDelay = _enemyStats.StartMoveDelay;
    }

    void Awake()
    {
        _spawnTime = Time.time;
        _canMove = false;
        // GetComponent は削除（DIで注入されるため）
    }

    void Start()
    {
        // プレイヤーを探す
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            _player = playerObj.transform;
        }

        _enemyHpManager.Initialize(Health);

        // DIで注入された _enemyStats を使用
        float randPosX = Random.Range(_enemyStats.MinRelativeSpawnPosX, _enemyStats.MaxRelativeSpawnPosX);
        float randPosY = Random.Range(_enemyStats.MinRelativeSpawnPosY, _enemyStats.MaxRelativeSpawnPosY);
        transform.position += new Vector3(_player.position.x + randPosX, _player.position.y + randPosY, 0);

        AudioManager.Instance.PlayAsync(SEType.Monster012, destroyCancellationToken).Forget();
    }

    void Update()
    {
        if (!_canMove)
        {
            if (Time.time - _spawnTime >= _startMoveDelay)
            {
                _canMove = true;
            }
            else
            {
                return;
            }
        }
        transform.Translate(Vector2.left * _speed * Time.deltaTime);
    }
}