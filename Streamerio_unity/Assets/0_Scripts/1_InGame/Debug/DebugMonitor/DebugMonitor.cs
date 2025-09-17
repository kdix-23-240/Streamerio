using UnityEngine;

/// <summary>
/// デバッグモニターは最初は非表示にする
/// </summary>
namespace DebugTool
{
    public class DebugMonitor : MonoBehaviour
    {
        void Start()
        {
            gameObject.SetActive(false);
        }
    }
}