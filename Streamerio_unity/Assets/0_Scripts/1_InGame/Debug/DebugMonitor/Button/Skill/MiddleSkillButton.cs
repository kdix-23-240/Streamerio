using UnityEngine;
using UnityEngine.UI;

public class MiddleSkillButton : MonoBehaviour
{
    [SerializeField] private SkillRandomActivator _skillRandomActivator; // SkillRandomActivatorの参照

    public void OnClick()
    {
        _skillRandomActivator.ActivateMiddleSkill();
    }
}