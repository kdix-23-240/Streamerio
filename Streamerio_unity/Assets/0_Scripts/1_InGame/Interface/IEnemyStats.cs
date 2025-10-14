using UnityEngine;

/// <summary>
/// 敵キャラクターの基本ステータスパラメーターを定義するインターフェース
/// </summary>
public interface IEnemyStats
{
    // 基本パラメーター
    float Speed { get; }
    float Power { get; }
    int Health { get; }
    float StartMoveDelay { get; }
    
    // 検出・追跡範囲
    float DetectionRange { get; }
    float StopRange { get; }
    
    // スポーン位置関連
    float MinRelativeSpawnPosX { get; }
    float MaxRelativeSpawnPosX { get; }
    float MinRelativeSpawnPosY { get; }
    float MaxRelativeSpawnPosY { get; }
}