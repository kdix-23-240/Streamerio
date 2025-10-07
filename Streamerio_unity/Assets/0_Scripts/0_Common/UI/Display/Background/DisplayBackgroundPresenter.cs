using System;
using System.Threading;
using Common.UI.Click;
using R3;
using R3.Triggers;

namespace Common.UI.Display.Background
{
    /// <summary>
    /// UI 背景の Presenter。
    /// - 背景の表示/非表示を制御
    /// - 背景クリックイベントを購読可能にする
    /// </summary>
    public class DisplayBackgroundPresenter : DisplayPresenterBase<IDisplayView>, IDisposable
    {
        private ObservableEventTrigger _clickTrigger;
        private IClickEventBinder _clickEventBinder;
        
        /// <summary>
        /// 背景クリック時のイベント購読用 Observable
        /// </summary>
        public Observable<Unit> OnClickAsObservable => _clickEventBinder.ClickEvent;
        
        private CancellationTokenSource _cts;

        public DisplayBackgroundPresenter(IDisplayView view, IClickEventBinder clickEventBinder): base(view)
        {
            _clickEventBinder = clickEventBinder;
            _cts = new CancellationTokenSource();
        }
        
        public override void Initialize()
        {
            base.Initialize();
            _clickEventBinder.BindClickEvent();
        }
        
        public void Dispose()
        {
            _clickEventBinder.Dispose();
            _cts?.Cancel();
            _cts?.Dispose();
        }
    }
}