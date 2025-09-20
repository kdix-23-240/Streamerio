using System.Collections.Generic;
using UnityEngine;

public class BulletPool : MonoBehaviour
{
    [SerializeField] private NormalBullet _bulletPrefab;
    [SerializeField] private GameObject _parentObject;
    private Queue<NormalBullet> _pool = new Queue<NormalBullet>();

    [SerializeField] private BulletScriptableObject _bulletScriptableObject;
    private int _poolSize;

    void Start()
    {
        _poolSize = _bulletScriptableObject.PoolSize;
        for (int i = 0; i < _poolSize; i++)
        {
            NormalBullet bullet = Instantiate(_bulletPrefab, _parentObject.transform);
            bullet.OnDespawn();
            _pool.Enqueue(bullet);
        }
    }

    public NormalBullet GetBullet()
    {
        if (_pool.Count > 0)
        {
            NormalBullet bullet = _pool.Dequeue();
            bullet.OnSpawn();
            return bullet;
        }
        else
        {
            NormalBullet bullet = Instantiate(_bulletPrefab, _parentObject.transform);
            return bullet;
        }
    }

    public void ReturnBullet(NormalBullet bullet)
    {
        bullet.OnDespawn();
        _pool.Enqueue(bullet);
    }
}