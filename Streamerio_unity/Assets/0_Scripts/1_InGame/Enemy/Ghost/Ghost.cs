using UnityEngine;

public class Ghost : MonoBehaviour
{
    [SerializeField] private float speed = 1.5f;
    
    private Transform _player;
    private Vector3 _baseScale;
    
    void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            _player = playerObj.transform;
        }
        
        // 重力無効化
        var rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.gravityScale = 0f;
        }
        _baseScale = transform.localScale;
    }
    
    void Update()
    {
        if (_player == null) return;
        
        Vector2 direction = (_player.position - transform.position).normalized;
        transform.position += (Vector3)(direction * speed * Time.deltaTime);

        float sign = direction.x < 0 ? -1f : 1f;
        transform.localScale = new Vector3(_baseScale.x * sign, _baseScale.y, _baseScale.z);
    }
}