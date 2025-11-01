using UnityEngine;

namespace Common.UI.Display.Window.Panel
{
    /// <summary>
    /// ページのつなぎ役 (Presenter)。
    /// - PagePanelView とやり取りするためのプレゼンター層
    /// - 現状は追加の処理を持たず、基底クラスの機能をそのまま利用する
    /// - 必要に応じてイベントハンドリングやロジックをここに追加可能
    /// </summary>
    [RequireComponent(typeof(PagePanelView))]
    public class PagePanelPresenter : DisplayPresenterBase<PagePanelView>
    {
        
    }
}