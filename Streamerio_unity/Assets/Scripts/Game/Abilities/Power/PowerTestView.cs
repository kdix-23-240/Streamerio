using UnityEngine;
using UnityEngine.UI;

public class PowerTestView : MonoBehaviour
{
    [SerializeField] private Text _powerAmount;
    public void Initialize(float initialPower)
    {
        UpdateView(initialPower);
    }

    public void UpdateView(float currentPower)
    {
        _powerAmount.text = currentPower.ToString();
    }
}