using UnityEngine;
using Cysharp.Threading.Tasks;

public class NormalBullet : MonoBehaviour
{
    private float _speed;
    [SerializeField] private BulletScriptableObject _bulletScriptableObject;
    void Awake()
    {
        _speed = _bulletScriptableObject.Speed;
    }
    void Update()
    {
        Move();
    }
    async void OnEnable()
    {
        await AutoDespawn();
    }

    public void OnSpawn()
    {
        gameObject.SetActive(true);
    }

    public void OnDespawn()
    {
        gameObject.SetActive(false);
    }

    private async UniTask AutoDespawn()
    {
        await UniTask.Delay(System.TimeSpan.FromSeconds(_bulletScriptableObject.Lifetime));
        OnDespawn();
    }
    private void Move()
    {
        transform.Translate(Vector2.right * _speed * Time.deltaTime);
    }
}