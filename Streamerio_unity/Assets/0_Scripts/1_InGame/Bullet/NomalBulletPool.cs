using System.Collections.Generic;
using UnityEngine;

public class BulletPool : MonoBehaviour
{
    [SerializeField] private NormalBullet _bulletPrefab;
    [SerializeField] private GameObject _parentObject;
    private List<NormalBullet> _pool = new List<NormalBullet>();

    [SerializeField] private BulletScriptableObject _bulletScriptableObject;
    private int _poolSize;

    void Start()
    {
        _poolSize = _bulletScriptableObject.PoolSize;
        for (int i = 0; i < _poolSize; i++)
        {
            NormalBullet bullet = Instantiate(_bulletPrefab, _parentObject.transform);
            bullet.OnDespawn();
            _pool.Add(bullet);
        }
    }

    public NormalBullet GetBullet()
    {
        foreach (var bullet in _pool)
        {
            if (!bullet.gameObject.activeInHierarchy)
            {
                bullet.OnSpawn();
                return bullet;
            }
        }
        // すべて使用中ならnull
        return null;
    }

    public void ReturnBullet(NormalBullet bullet)
    {
        bullet.OnDespawn();
        // Listなので再追加不要
    }
}