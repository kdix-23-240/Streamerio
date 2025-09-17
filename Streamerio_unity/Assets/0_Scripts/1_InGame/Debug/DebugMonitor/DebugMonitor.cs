using System.Threading.Tasks;
using UnityEngine;

/// <summary>
/// デバッグモニターは最初は非表示にする
/// </summary>
public class DebugMonitorManager : MonoBehaviour
{
    void Start()
    {
        gameObject.SetActive(false);
    }
}