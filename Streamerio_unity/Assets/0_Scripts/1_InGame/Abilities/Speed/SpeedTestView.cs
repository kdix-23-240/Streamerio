using UnityEngine;
using UnityEngine.UI;

public class SpeedTestView : MonoBehaviour
{
    [SerializeField] private Text _speedAmount;
    public void Initialize(float initialSpeed)
    {
        UpdateView(initialSpeed);
    }

    public void UpdateView(float currentSpeed)
    {
        _speedAmount.text = currentSpeed.ToString();
    }
}