using System.Threading;
using Alchemy.Inspector;
using Cysharp.Threading.Tasks;
using R3;
using R3.Triggers;
using UnityEngine;

namespace Common.UI.Display.Overlay
{
    [RequireComponent(typeof(ObservableEventTrigger))]
    public class OverlayPresenterBase<TView>: DisplayPresenterBase<TView>, IOverlay
        where TView: OverlayViewBase
    {
        [SerializeField, ReadOnly]
        private ObservableEventTrigger _clickTrigger;
        
        private Subject<Unit> _clickEvent;
        /// <summary>
        /// クリックされた時のイベント
        /// </summary>
        protected Observable<Unit> OnClickAsObservable => _clickEvent;

        private CancellationTokenSource _cts;
        
#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();
            _clickTrigger ??= GetComponent<ObservableEventTrigger>();
        }
#endif
        
        public override void Initialize()
        {
            _clickEvent = new Subject<Unit>();
            base.Initialize();
        }
        
        private void BindClickEvent()
        {
            _cts = CancellationTokenSource.CreateLinkedTokenSource(destroyCancellationToken);
            
            _clickTrigger.OnPointerClickAsObservable()
                .Subscribe(_ =>
                {
                    _clickEvent.OnNext(Unit.Default);
                })
                .RegisterTo(_cts.Token);
        }

        public override async UniTask ShowAsync(CancellationToken ct)
        {
            await base.ShowAsync(ct);
            BindClickEvent();
        }

        public override void Show()
        {
            base.Show();
            BindClickEvent();
        }
        
        public override async UniTask HideAsync(CancellationToken ct)
        {
            _cts?.Cancel();
            _cts?.Dispose();
            await base.HideAsync(ct);
        }

        public override void Hide()
        {
            _cts?.Cancel();
            _cts?.Dispose();
            base.Hide();
        }
    }
}