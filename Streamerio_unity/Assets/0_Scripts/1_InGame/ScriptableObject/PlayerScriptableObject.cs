using UnityEngine;

[CreateAssetMenu(fileName = "PlayerScriptableObject", menuName = "PlayerScriptableObject")]
public class PlayerScriptableObject : ScriptableObject
{
    public float InitialHp = 10;
    public float MaxHp = 50;
    public float InitialSpeed = 10;
    public float InitialPower = 10;
}