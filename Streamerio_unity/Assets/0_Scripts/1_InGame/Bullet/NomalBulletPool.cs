using System.Collections.Generic;
using UnityEngine;

public class BulletPool : MonoBehaviour
{
    public NomalBullet bulletPrefab;
    public int poolSize = 10;
    private Queue<NomalBullet> pool = new Queue<NomalBullet>();

    void Start()
    {
        for (int i = 0; i < poolSize; i++)
        {
            NomalBullet bullet = Instantiate(bulletPrefab, transform);
            bullet.OnDespawn();
            pool.Enqueue(bullet);
        }
    }

    public NomalBullet GetBullet()
    {
        if (pool.Count > 0)
        {
            NomalBullet bullet = pool.Dequeue();
            bullet.OnSpawn();
            return bullet;
        }
        else
        {
            NomalBullet bullet = Instantiate(bulletPrefab, transform);
            return bullet;
        }
    }

    public void ReturnBullet(NomalBullet bullet)
    {
        bullet.OnDespawn();
        pool.Enqueue(bullet);
    }
}