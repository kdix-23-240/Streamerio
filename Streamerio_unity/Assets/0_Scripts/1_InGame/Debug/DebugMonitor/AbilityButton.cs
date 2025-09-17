using UnityEngine;
using UnityEngine.UI;

public class AbilityButton : MonoBehaviour
{
    [SerializeField] private MonoBehaviour _abilityComponent;
    [SerializeField] private AbilityType _abilityType;
    [SerializeField] private bool _isIncrease;
    [SerializeField] private PlayerScriptableObject _playerData;

    private IAbility _ability;

    private void Awake()
    {
        // MonoBehaviourからIAbilityを取得
        _ability = _abilityComponent as IAbility;
    }

    public void OnClick()
    {
        Count();
    }

    protected void Count()
    {
        if (_ability == null) return;

        float amount = 0f;
        switch (_abilityType)
        {
            case AbilityType.Hp:
                amount = _playerData.HpDiff;
                break;
            case AbilityType.Power:
                amount = _playerData.PowerDiff;
                break;
            case AbilityType.Speed:
                amount = _playerData.SpeedDiff;
                break;
        }

        Debug.Log($"AbilityButton: {_abilityType} {( _isIncrease ? "Increase" : "Decrease" )} by {amount}");

        if (_isIncrease)
        {
            _ability.Increase(amount);
        }
        else
        {
            _ability.Decrease(amount);
        }
    }
}

public enum AbilityType
{
    Hp,
    Power,
    Speed
}