using UnityEngine;

public class BulletShooter : MonoBehaviour
{
    [SerializeField] private BulletPool _bulletPool;
    [SerializeField] private Transform _firePoint;

    public void Shoot()
    {
        Debug.Log("Shoot");
        NormalBullet bullet = _bulletPool.GetBullet();
        if (bullet != null)
        {
            bullet.transform.position = _firePoint.position;
            bullet.transform.rotation = _firePoint.rotation;
            bullet.gameObject.SetActive(true);
            bullet.OnSpawn();
        }
    }
}