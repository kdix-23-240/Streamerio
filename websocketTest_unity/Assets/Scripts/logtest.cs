using UnityEngine;
using TMPro;

public class logtest : MonoBehaviour
{
    [SerializeField]
    private TMP_Text logText;

    void Start()
    {
        Application.logMessageReceived += OnLogMessageReceived;
    }
    private void OnLogMessageReceived(string logString, string stackTrace, LogType type)
    {
        logText.text = logString;
    }
}
