using System.Threading;
using Alchemy.Inspector;
using Common.Audio;
using Cysharp.Threading.Tasks;
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
        [SerializeField, ReadOnly]
        private ObservableEventTrigger _clickTrigger;
        
        private Subject<Unit> _clickEvent;

        /// <summary>
        /// クリックされた時のイベント（購読用）
        /// </summary>
        protected Observable<Unit> OnClickAsObservable => _clickEvent;

        private CancellationTokenSource _cts;
        
#if UNITY_EDITOR
        /// <summary>
        /// エディタ上でコンポーネント参照を自動補完
        /// </summary>
        protected override void OnValidate()
        {
            base.OnValidate();
            _clickTrigger ??= GetComponent<ObservableEventTrigger>();
        }
#endif
        
        /// <summary>
        /// 初期化処理。
        /// - クリック通知用 Subject を生成
        /// - 基底クラスの初期化を呼び出す
        /// </summary>
        public override void Initialize()
        {
            _clickEvent = new Subject<Unit>();
            base.Initialize();
        }
        
        /// <summary>
        /// クリックイベントの購読をセットアップ。
        /// - 効果音を再生
        /// - Subject に通知を流す
        /// - Show/Hide ごとに購読を張り替えるため、CancellationToken を管理
        /// </summary>
        private void BindClickEvent()
        {
            _cts = CancellationTokenSource.CreateLinkedTokenSource(destroyCancellationToken);
            
            _clickTrigger.OnPointerClickAsObservable()
                .Subscribe(_ =>
                {
                    AudioManager.Instance.PlayAsync(SEType.SNESRPG01, _cts.Token).Forget();
                    _clickEvent.OnNext(Unit.Default);
                })
                .RegisterTo(_cts.Token);
        }

        /// <summary>
        /// アニメーション付きで表示。
        /// 表示完了後にクリックイベント購読を開始。
        /// </summary>
        public override async UniTask ShowAsync(CancellationToken ct)
        {
            await base.ShowAsync(ct);
            BindClickEvent();
        }

        /// <summary>
        /// 即時表示。
        /// 表示後にクリックイベント購読を開始。
        /// </summary>
        public override void Show()
        {
            base.Show();
            BindClickEvent();
        }
        
        /// <summary>
        /// アニメーション付きで非表示。
        /// 購読をキャンセル＆解放後、基底処理を実行。
        /// </summary>
        public override async UniTask HideAsync(CancellationToken ct)
        {
            _cts?.Cancel();
            _cts?.Dispose();
            await base.HideAsync(ct);
        }

        /// <summary>
        /// 即時非表示。
        /// 購読をキャンセル＆解放後、基底処理を実行。
        /// </summary>
        public override void Hide()
        {
            _cts?.Cancel();
            _cts?.Dispose();
            base.Hide();
        }
    }
}