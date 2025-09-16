using UnityEngine;

public class HpTestView : MonoBehaviour
{
	public void Initialize(float initialHp)
	{
		UpdateView(initialHp);
	}

	public void UpdateView(float currentHp)
	{
		Debug.Log($"Hp updated to {currentHp}");
	}
}