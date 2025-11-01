using R3;
using R3.Triggers;
using UnityEngine;

namespace Common.UI.Display.Overlay
{
    /// <summary>
    /// Overlay 系 Display の基盤 Presenter。
    /// - OverlayViewBase を制御対象にする
    /// - クリックイベントを購読し、効果音を再生＆通知する
    /// - Show/Hide のタイミングでイベント購読を管理（生成/解放）
    /// </summary>
    [RequireComponent(typeof(ObservableEventTrigger), typeof(CommonOverlayView))]
    public class OverlayPresenterBase : DisplayPresenterBase<CommonOverlayView>
    {
        /// <summary>
        /// クリックされた時のイベント（購読用）
        /// </summary>
        protected Observable<Unit> OnClickAsObservable => CommonView.Background.OnClickAsObservable;
    }
}