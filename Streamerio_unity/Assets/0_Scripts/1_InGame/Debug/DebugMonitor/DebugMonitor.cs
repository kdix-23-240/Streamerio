using System.Threading.Tasks;
using UnityEngine;

/// <summary>
/// デバッグモニターは最初は非表示にする
/// </summary>
namespace DebugTool
{
    public class DebugMonitor : MonoBehaviour
    {
        async void Start()
        {
            gameObject.SetActive(false);
        }
    }
}