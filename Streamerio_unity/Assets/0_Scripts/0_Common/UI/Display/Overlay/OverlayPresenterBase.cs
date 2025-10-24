namespace Common.UI.Display.Overlay
{
    public interface IOverlay : IDisplay
    {
        
    }
    
    /// <summary>
    /// Overlay 系 Display の基盤 Presenter。
    /// - OverlayViewBase を制御対象にする
    /// - クリックイベントを購読し、効果音を再生＆通知する
    /// - Show/Hide のタイミングでイベント購読を管理（生成/解放）
    /// </summary>
    public class OverlayPresenterBase<TView, TContext> : DisplayPresenterBase<TView, TContext>
        where TView : IOverlayView
        where TContext : CommonOverlayContext<TView>
    {
        protected override void AttachContext(TContext context)
        {
            View = context.View;
        }
    }
    
    public class CommonOverlayContext<TView>
        where TView : IOverlayView
    {
        public TView View;
    }
}