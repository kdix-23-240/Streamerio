using Common.UI.Dialog;
using Common.UI.Display.Overlay;
using Common.UI.Display.Window;

namespace Common.UI.Display
{
    /// <summary>
    /// 各 UI マネージャーをまとめて初期化するシングルトン。
    /// - WindowManager, OverlayManager, DialogManager を一括で起動
    /// - ゲーム開始時やシーン遷移時に呼び出すことで UI 管理基盤を確実に立ち上げる
    /// </summary>
    public class DisplayBooster : SingletonBase<DisplayBooster>
    {
        /// <summary>
        /// UI 管理マネージャーを全て初期化。
        /// - Window（ウィンドウUIの管理）
        /// - Overlay（オーバーレイUIの管理）
        /// - Dialog（ダイアログUIの管理）
        /// </summary>
        public void Boost()
        {
            WindowManager.Instance.Initialize();
            OverlayManager.Instance.Initialize();
            DialogManager.Instance.Initialize();
        }
    }
}