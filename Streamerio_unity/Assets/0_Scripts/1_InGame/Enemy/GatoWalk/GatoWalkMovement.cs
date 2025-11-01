using Common.Audio;
using Cysharp.Threading.Tasks;
using UnityEngine;
using VContainer;

public class GatoWalkMovement : MonoBehaviour
{
    [Header("移動設定")]
    [SerializeField] private float speed = 4f; // 高速移動
    [SerializeField] private float jumpForce = 8f;
    [SerializeField] private float jumpInterval = 2f;
    [SerializeField] private float attackCooldown = 0.6f;
    
    private Rigidbody2D _rigidbody;
    private bool _isGrounded = true;
    private float _jumpTimer = 0f;
    private EnemyAttackManager _attackManager;
    private float _lastAttackTime = -999f;
    private Transform _player;
    
    private IAudioFacade _audioFacade;

    [Inject]
    public void Construct(IAudioFacade audioFacade)
    {
        _audioFacade = audioFacade;
    }

    void Start()
    {
        _rigidbody = GetComponent<Rigidbody2D>();
        if (_rigidbody == null)
        {
            _rigidbody = gameObject.AddComponent<Rigidbody2D>();
        }

        // プレイヤーを探す
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            _player = playerObj.transform;
        }

        // 物理設定
            _rigidbody.gravityScale = 2f;
        _rigidbody.freezeRotation = true;

        _attackManager = GetComponent<EnemyAttackManager>();
        _jumpTimer = jumpInterval;
        
        transform.position += new Vector3(_player.position.x + 10, _player.position.y + 1, 0); // 少し上にずらして生成
        AudioManager.Instance.AudioFacade.PlayAsync(SEType.Monster012, destroyCancellationToken).Forget();
    }
    
    void Update()
    {
        HandleMovement();
        HandleJump();
    }
    
    private void HandleMovement()
    {
        // 常に左方向に高速移動
        Vector2 velocity = _rigidbody.linearVelocity;
        velocity.x = -speed;
        _rigidbody.linearVelocity = velocity;
    }
    
    private void HandleJump()
    {
        _jumpTimer -= Time.deltaTime;
        
        // 接地時かつジャンプタイマーが0以下の場合ジャンプ
        if (_isGrounded && _jumpTimer <= 0f)
        {
            Jump();
            _jumpTimer = jumpInterval;
        }
    }
    
    private void Jump()
    {
        Vector2 velocity = _rigidbody.linearVelocity;
        velocity.y = jumpForce;
        _rigidbody.linearVelocity = velocity;
        _isGrounded = false;
        
        Debug.Log("GatoWalk jumped!");
    }
    
    private void OnCollisionEnter2D(Collision2D collision)
    {
        // 地面判定（下方向の接触）
        if (collision.contacts.Length > 0)
        {
            Vector2 normal = collision.contacts[0].normal;
            if (normal.y > 0.7f) // 上向きの法線 = 地面
            {
                _isGrounded = true;
            }
        }
    }
}