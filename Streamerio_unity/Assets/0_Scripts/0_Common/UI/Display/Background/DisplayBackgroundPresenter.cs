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
    /// UI 背景の Presenter。
    /// - 背景の表示/非表示を制御
    /// - 背景クリックイベントを購読可能にする
    /// </summary>
    [RequireComponent(typeof(DisplayBackgroundView), typeof(ObservableEventTrigger), typeof(ClickEventBinder))]
    public class DisplayBackgroundPresenter : DisplayPresenterBase<DisplayBackgroundView>
    {
        [SerializeField, ReadOnly]
        private ObservableEventTrigger _clickTrigger;

        [SerializeField, ReadOnly]
        private ClickEventBinder _clickEventBinder;
        
        /// <summary>
        /// 背景クリック時のイベント購読用 Observable
        /// </summary>
        public Observable<Unit> OnClickAsObservable => _clickEventBinder.ClickEvent;
        
        private CancellationTokenSource _cts;

#if UNITY_EDITOR
        /// <summary>
        /// エディタ上でコンポーネント参照を自動補完
        /// </summary>
        protected override void OnValidate()
        {
            base.OnValidate();
            
            _clickTrigger ??= GetComponent<ObservableEventTrigger>();
            _clickEventBinder ??= GetComponent<ClickEventBinder>();
        }
#endif
        
        /// <summary>
        /// 初期化処理。
        /// - ClickEventBinder を初期化
        /// - 基底クラスの初期化を呼ぶ
        /// </summary>
        public override void Initialize()
        {
            _clickEventBinder.Initialize();
            base.Initialize();
        }
        
        /// <summary>
        /// アニメーション付き表示。
        /// - クリックイベント購読をバインド
        /// - 基底クラスの表示処理を呼ぶ
        /// </summary>
        public override async UniTask ShowAsync(CancellationToken ct)
        {
            _clickEventBinder.BindClickEvent(_clickTrigger.OnPointerClickAsObservable());
            await base.ShowAsync(ct);
        }
        
        /// <summary>
        /// 即時表示。
        /// - クリックイベント購読をバインド
        /// - 基底クラスの表示処理を呼ぶ
        /// </summary>
        public override void Show()
        {
            _clickEventBinder.BindClickEvent(_clickTrigger.OnPointerClickAsObservable());
            base.Show();
        }
        
        /// <summary>
        /// アニメーション付き非表示。
        /// - 基底クラスの非表示処理
        /// - クリックイベント購読を破棄
        /// </summary>
        public override async UniTask HideAsync(CancellationToken ct)
        {
            await base.HideAsync(ct);
            _clickEventBinder.Dispose();
        }
        
        /// <summary>
        /// 即時非表示。
        /// - 基底クラスの非表示処理
        /// - クリックイベント購読を破棄
        /// </summary>
        public override void Hide()
        {
            base.Hide();
            _clickEventBinder.Dispose();
        }
    }
}