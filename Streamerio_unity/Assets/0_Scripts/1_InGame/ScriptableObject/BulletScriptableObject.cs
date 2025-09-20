using UnityEngine;

[CreateAssetMenu(fileName = "BulletScriptableObject", menuName = "SO/InGame/Bullet/BulletScriptableObject")]
public class BulletScriptableObject : ScriptableObject
{
    public GameObject BulletPrefab;
    public int PoolSize = 10;
    public float Speed = 0.1f;
    public float Damage = 10f;
    public float SizeRate = 1.0f;
    public float Lifetime = 5f;
    public float Span = 0.5f;
}