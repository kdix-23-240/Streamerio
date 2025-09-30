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
    /// - ユーザーのクリックをトリガーにして処理を進める
    /// - タイトル → メインメニュー(ブックウィンドウ) → タイトル再表示 の流れを制御
    /// </summary>
    [RequireComponent(typeof(TitleScreenView), typeof(ObservableEventTrigger), typeof(ClickEventBinder))]
    public class TitleScreenPresenter : DisplayPresenterBase<TitleScreenView>
    {
        [SerializeField, ReadOnly]
        private ObservableEventTrigger _clickTrigger;
        [SerializeField, ReadOnly]
        private ClickEventBinder _clickEventBinder;

#if UNITY_EDITOR
        /// <summary>
        /// エディタ上で参照を自動補完
        /// </summary>
        protected override void OnValidate()
        {
            base.OnValidate();
            _clickTrigger ??= GetComponent<ObservableEventTrigger>();
            _clickEventBinder ??= GetComponent<ClickEventBinder>();
        }
#endif

        public override void Initialize()
        {
            _clickEventBinder.Initialize();
            base.Initialize();
        }

        /// <summary>
        /// イベント購読設定。
        /// - タイトルを非表示にしてブックウィンドウを開く
        /// - メニューが閉じられたら再びタイトルを表示
        /// </summary>
        protected override void Bind()
        {
            base.Bind();
            
            _clickEventBinder.BindClickEvent(_clickTrigger.OnPointerClickAsObservable());
            
            _clickEventBinder.ClickEvent
                .SubscribeAwait(async (_, ct) =>
                {
                    // タイトル画面を非表示にする
                    await HideAsync(ct);
                    
                    // ブックウィンドウを開いて、閉じられるまで待機
                    await WindowManager.Instance.OpenAndWaitCloseAsync<OutGameBookWindowPresenter>(ct);

                    // ブックウィンドウを閉じたら再びタイトル画面を表示
                    await ShowAsync(ct);
                })
                .RegisterTo(destroyCancellationToken);
        }
    }
}