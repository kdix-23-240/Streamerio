using UnityEngine;
using UnityEngine.UI;

public class JumpPowerTestView : MonoBehaviour
{
    [SerializeField] private Text _jumpPowerAmount;
    public void Initialize(float initialJumpPower)
    {
        UpdateView(initialJumpPower);
    }

    public void UpdateView(float currentJumpPower)
    {
        _jumpPowerAmount.text = currentJumpPower.ToString();
    }
}