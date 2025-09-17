using UnityEngine;

[CreateAssetMenu(fileName = "PlayerScriptableObject", menuName = "PlayerScriptableObject")]
public class PlayerScriptableObject : ScriptableObject
{
    [Header("Player Initial Setting")]
    public float InitialHp = 100;
    public float InitialPower = 100;
    public float InitialSpeed = 100;
    public float InitialJumpPower = 10;

    [Header("Player Ability Diff")]
    public float HpDiff = 10;
    public float PowerDiff = 2f;
    public float SpeedDiff = 2f;
    public float JumpPowerDiff = 2f;

    [Header("Player Ability Max Value")]
    public float MaxHp = 100;
    public float MaxPower = 20;
    public float MaxSpeed = 20;
    public float MaxJumpPower = 20;
}