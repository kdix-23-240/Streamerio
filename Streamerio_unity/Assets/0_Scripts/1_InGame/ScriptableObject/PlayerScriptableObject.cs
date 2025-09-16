using UnityEngine;

[CreateAssetMenu(fileName = "PlayerScriptableObject", menuName = "PlayerScriptableObject")]
public class PlayerScriptableObject : ScriptableObject
{
    [Header("Player Initial Setting")]
    public float InitialHp = 100;
    public float InitialPower = 100;
    public float InitialSpeed = 100;

    [Header("Player Ability Diff")]
    public float HpDiff = 10;
    public float PowerDiff = 5;
    public float SpeedDiff = 2;

    [Header("Player Ability Max Value")]
    public float MaxHp = 100;
    public float MaxPower = 50;
    public float MaxSpeed = 20;
}