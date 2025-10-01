using UnityEngine;

namespace Common.UI.Display.Window
{
    /// <summary>
    /// Window 系 UI の Presenter 基底クラス。
    /// - 共通の View 型として CommonWindowView を利用
    /// - 個別の Window Presenter はこのクラスを継承して実装する
    /// - 共通処理は DisplayPresenterBase に集約されているため、
    ///   ここでは特に追加実装はせず「マーカー的役割」を担う
    /// </summary>
    [RequireComponent(typeof(CommonWindowView))]
    public abstract class WindowPresenterBase : DisplayPresenterBase<CommonWindowView>
    {
        
    }
}