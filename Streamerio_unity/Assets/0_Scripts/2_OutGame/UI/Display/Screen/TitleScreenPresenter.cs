using Alchemy.Inspector;
using Common.UI.Click;
using Common.UI.Display;
using Common.UI.Display.Window;
using OutGame.UI.Display.Window;
using R3;
using R3.Triggers;
using UnityEngine;

namespace OutGame.UI.Display.Screen
{
    /// <summary>
    /// タイトル画面（スクリーン）のプレゼンター。
    /// - TitleScreenView と ClickEventBinder を制御
    /// - ユーザークリックをトリガーに、タイトル非表示 → ブックウィンドウ表示 の遷移を行う
    /// </summary>
    [RequireComponent(typeof(TitleScreenView), typeof(ObservableEventTrigger), typeof(ClickEventBinder))]
    public class TitleScreenPresenter : DisplayPresenterBase<TitleScreenView>
    {
        [SerializeField, ReadOnly]
        private ObservableEventTrigger _clickTrigger;   // クリックなどの UI イベントを Observable 化するための Trigger
        [SerializeField, ReadOnly]
        private ClickEventBinder _clickEventBinder;     // クリックを間引き＆SE再生など共通処理付きで通知してくれるバインダ

#if UNITY_EDITOR
        /// <summary>
        /// エディタ上で未参照なら自動補完。
        /// </summary>
        protected override void OnValidate()
        {
            base.OnValidate();
            _clickTrigger ??= GetComponent<ObservableEventTrigger>();
            _clickEventBinder ??= GetComponent<ClickEventBinder>();
        }
#endif

        /// <summary>
        /// 初期化。
        /// - ClickEventBinder の内部状態を初期化（間引き間隔や SE 再生などの準備）
        /// - 基底の Initialize（View の初期化など）も実行
        /// </summary>
        public override void Initialize()
        {
            _clickEventBinder.Initialize();
            base.Initialize();
        }

        /// <summary>
        /// イベント購読のセットアップ。
        /// - 画面全体クリック（_clickTrigger）を ClickEventBinder にバインド
        /// - クリックが発火したら：タイトルを隠す → ブックウィンドウを開く
        ///   （※ブックウィンドウは閉じ待ちはせず、開くところまで）
        /// </summary>
        protected override void Bind()
        {
            base.Bind();

            // クリック系の Unity イベントを ClickEventBinder に橋渡し
            // （間引きや SE 再生など、共通のクリック処理が入る）
            _clickEventBinder.BindClickEvent(_clickTrigger.OnPointerClickAsObservable());

            // 実際のクリック時ロジック
            _clickEventBinder.ClickEvent
                .SubscribeAwait(async (_, ct) =>
                {
                    // タイトル画面をフェードアウト等で非表示
                    await HideAsync(ct);

                    // メインメニュー（ブックウィンドウ）を表示
                    // ※閉じられるまで待たず、ここでは「開く」だけ
                    await WindowManager.Instance.OpenDisplayAsync<OutGameBookWindowPresenter>(ct);
                })
                .RegisterTo(destroyCancellationToken); // 破棄時に購読解除
        }
    }
}
