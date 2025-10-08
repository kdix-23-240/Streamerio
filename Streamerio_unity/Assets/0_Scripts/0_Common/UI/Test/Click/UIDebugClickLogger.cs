using UnityEngine;
using UnityEngine.EventSystems;

namespace Common.UI.Test
{
    /// <summary>
    /// UI のクリック対象をデバッグ出力するスクリプト。
    /// - Update 内でマウス左クリックを検知
    /// - EventSystem.current から現在選択中の UI を取得
    /// - 名前を Debug.Log に出力
    /// </summary>
    public class UIDebugClickLogger : MonoBehaviour
    {
        void Update()
        {
            // 左クリックを検知
            if (Input.GetMouseButtonDown(0))
            {
                // EventSystem がアクティブか確認
                if (EventSystem.current == null)
                {
                    Debug.LogWarning("[UIDebugClickLogger] EventSystem が存在しません。");
                    return;
                }

                // 現在クリックされた UI オブジェクトを取得
                var selected = EventSystem.current.currentSelectedGameObject;

                if (selected != null)
                {
                    Debug.Log($"[UIDebugClickLogger] Clicked: {selected.name}");
                }
                else
                {
                    Debug.Log("[UIDebugClickLogger] UI 以外がクリックされました。");
                }
            }
        }
    }

}