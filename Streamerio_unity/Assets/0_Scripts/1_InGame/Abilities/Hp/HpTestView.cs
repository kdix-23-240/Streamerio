using UnityEngine;
using UnityEngine.UI;

public class HpTestView : MonoBehaviour
{
	[SerializeField] private Text _hpAmount;
    public void Initialize(float initialHp)
	{
		UpdateView(initialHp);
	}

	public void UpdateView(float currentHp)
	{
		_hpAmount.text = currentHp.ToString();
    }
}