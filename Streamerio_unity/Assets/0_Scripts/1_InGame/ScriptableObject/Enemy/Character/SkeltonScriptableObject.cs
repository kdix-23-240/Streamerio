using UnityEngine;

[CreateAssetMenu(fileName = "SkeltonScriptableObject", menuName = "SO/InGame/Enemy/Character/Skelton")]
public class SkeltonScriptableObject : ScriptableObject, IEnemyStats
{
    [Header("Base Parameter")]
    public float Speed = 1.5f;
    public float Power = 10;
    public int Health = 30;
    public float StartMoveDelay = 0.6f;

    [Header("Relative Spawn Position With Player")]
    public float MinRelativeSpawnPosX = 10;
    public float MaxRelativeSpawnPosX = 15;
    public float MinRelativeSpawnPosY = 0;
    public float MaxRelativeSpawnPosY = 0;

    [Header("Chase Range")]
    public float DetectionRange = 8f;
    public float StopRange = 0.5f;

    // IEnemyStats の実装（プロパティとして公開）
    float IEnemyStats.Speed => Speed;
    float IEnemyStats.Power => Power;
    int IEnemyStats.Health => Health;
    float IEnemyStats.StartMoveDelay => StartMoveDelay;
    float IEnemyStats.DetectionRange => DetectionRange;
    float IEnemyStats.StopRange => StopRange;
    float IEnemyStats.MinRelativeSpawnPosX => MinRelativeSpawnPosX;
    float IEnemyStats.MaxRelativeSpawnPosX => MaxRelativeSpawnPosX;
    float IEnemyStats.MinRelativeSpawnPosY => MinRelativeSpawnPosY;
    float IEnemyStats.MaxRelativeSpawnPosY => MaxRelativeSpawnPosY;
}