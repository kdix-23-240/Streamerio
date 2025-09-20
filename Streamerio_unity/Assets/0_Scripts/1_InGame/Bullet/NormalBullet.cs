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
    async void Start()
    {
        await AutoDespawn();
    }
    void Update()
    {
        transform.Translate(Vector3.forward * _speed * Time.deltaTime);
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
}