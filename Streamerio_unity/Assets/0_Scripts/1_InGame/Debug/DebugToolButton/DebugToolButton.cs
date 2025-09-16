using UnityEngine;
using UnityEngine.UI;

public class DebugToolButton : MonoBehaviour
{
    [SerializeField] private GameObject _debugMonitor;

    public void OnClick()
    {
        _debugMonitor.SetActive(!_debugMonitor.activeSelf);
    }
}