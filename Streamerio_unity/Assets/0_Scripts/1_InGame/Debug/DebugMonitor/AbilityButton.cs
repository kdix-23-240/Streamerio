using UnityEngine;
using UnityEngine.UI;

namespace DebugMonitor
{
    public class AbilityButton : MonoBehaviour
    {
        [SerializeField] private MonoBehaviour _abilityComponent; // IAbilityを実装したプレゼンターオブジェクト
        [SerializeField] private AbilityType _abilityType; // 調整する能力の種類
        [SerializeField] private bool _isIncrease; // 増加させるか減少させるか
        [SerializeField] private PlayerScriptableObject _playerData; // プレイヤーデータのスクリプタブルオブジェクト
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

            Debug.Log($"AbilityButton: {_abilityType} {(_isIncrease ? "Increase" : "Decrease")} by {amount}");

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
}