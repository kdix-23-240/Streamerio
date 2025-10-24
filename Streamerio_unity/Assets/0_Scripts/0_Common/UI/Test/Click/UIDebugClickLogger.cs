// ============================================================================
// モジュール概要: UI 上でクリックした要素をログ出力し、EventSystem 設定のデバッグを支援する。
// 外部依存: UnityEngine、UnityEngine.EventSystems。
// 使用例: デバッグシーンで UIDebugClickLogger をアタッチし、クリック対象の GameObject 名を即座に確認する。
// ============================================================================

using UnityEngine;
using UnityEngine.EventSystems;

namespace Common.UI.Test
{
    /// <summary>
    /// UI のクリック対象をデバッグ出力するスクリプト。
    /// - Update 内でマウス左クリックを検知
    /// - EventSystem.current から現在選択中の UI を取得
    /// - 名前を Debug.Log に出力
    /// <para>
    /// 【理由】UI 操作が効かない際に、イベントがどこへ飛んでいるのかを素早く確認するため。
    /// </para>
    /// </summary>
    public class UIDebugClickLogger : MonoBehaviour
    {
        /// <summary>
        /// 【目的】フレームごとにクリック入力を監視し、対象 UI をログ出力する。
        /// 【理由】UI 操作トラブル時の原因調査を容易にするため。
        /// </summary>
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
