using System.Threading;
using Alchemy.Inspector;
using Common.UI.Click;
using Cysharp.Threading.Tasks;
using R3;
using R3.Triggers;
using UnityEngine;

namespace Common.UI.Display.Background
{
    /// <summary>
    /// UIの背景
    /// </summary>
    [RequireComponent(typeof(DisplayBackgroundView), typeof(ObservableEventTrigger), typeof(ClickEventBinder))]
    public class DisplayBackgroundPresenter: DisplayPresenterBase<DisplayBackgroundView>
    {
        [SerializeField, ReadOnly]
        private ObservableEventTrigger _clickTrigger;
        [SerializeField, ReadOnly]
        private ClickEventBinder _clickEventBinder;
        
        /// <summary>
        /// 背景をクリックした時のイベント
        /// </summary>
        public Observable<Unit> OnClickAsObservable => _clickEventBinder.ClickEvent;
        
        private CancellationTokenSource _cts;

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();
            
            _clickTrigger ??= GetComponent<ObservableEventTrigger>();
            _clickEventBinder ??= GetComponent<ClickEventBinder>();
        }
#endif
        
        /// <summary>
        /// 初期化
        /// </summary>
        public override void Initialize()
        {
            _clickEventBinder.Initialize();
            base.Initialize();
        }
        
        public override async UniTask ShowAsync(CancellationToken ct)
        {
            _clickEventBinder.BindClickEvent(_clickTrigger.OnPointerClickAsObservable());
            await base.ShowAsync(ct);
        }
        
        public override void Show()
        {
            _clickEventBinder.BindClickEvent(_clickTrigger.OnPointerClickAsObservable());
            base.Show();
        }
        
        public override async UniTask HideAsync(CancellationToken ct)
        {
            await base.HideAsync(ct);
            _clickEventBinder.Dispose();
        }
        
        public override void Hide()
        {
            base.Hide();
            _clickEventBinder.Dispose();
        }
    }
}