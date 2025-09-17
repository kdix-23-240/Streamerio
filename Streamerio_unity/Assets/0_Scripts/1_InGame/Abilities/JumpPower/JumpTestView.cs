using UnityEngine;
using UnityEngine.UI;

public class JumpTestView : MonoBehaviour
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