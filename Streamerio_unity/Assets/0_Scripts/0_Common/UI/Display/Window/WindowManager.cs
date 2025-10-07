using UnityEngine;

namespace Common.UI.Display.Window
{
    /// <summary>
    /// Window 系 UI を統括するマネージャ。
    /// - DisplayManagerBase を継承し、Window 専用のサービス (WindowService) を生成する
    /// - Repository から UI を生成/取得し、スタック制御（開閉管理）を行う
    /// </summary>
    public class WindowManager : DisplayManagerBase<WindowRepositorySO, WindowManager>
    {
        /// <summary>
        /// Window 用の IDisplayService を生成する。
        /// - Repository と 親 Transform を引数に WindowService を返す
        /// </summary>
        protected override IDisplayService InstanceDisplayService(WindowRepositorySO repository, Transform parent)
        {
            return new WindowService(repository, parent);
        }
    }
}