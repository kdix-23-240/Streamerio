// モジュール概要:
// 背景クリック UI 専用の LifetimeScope を構築し、DisplayBackgroundPresenter を初期化する。
// 依存関係: ObservableEventTrigger でクリックイベントを取得し、IAudioFacade による SE 再生を組み合わせる。

using Alchemy.Inspector;
using Common.Audio;
using Common.UI.Click;
using R3.Triggers;
using UnityEngine;
using UnityEngine.EventSystems;
using VContainer;
using VContainer.Unity;

namespace Common.UI.Display.Background
{
    /// <summary>
    /// 【目的】背景クリック UI に必要な依存を登録し、Presenter を配線する。
    /// 【理由】背景だけを独立モジュール化して動的生成しても、依存解決が破綻しないようにするため。
    /// </summary>
    [RequireComponent(typeof(ObservableEventTrigger))]
    public class DisplayBackGroundLifetimeScope : DisplayLifetimeScopeBase<IDisplayBackground, DisplayBackgroundPresenter, IDisplayBackgroundView, DisplayBackgroundContext>
    {
        /// <summary>
        /// 【目的】背景クリック検知用の ObservableEventTrigger を保持する。
        /// 【理由】ClickEventBinder が購読するストリームを安全に提供するため。
        /// </summary>
        [SerializeField, ReadOnly]
        private ObservableEventTrigger _clickTrigger;

        [Header("Settings")]
        [Tooltip("背景クリック時に再生する SE")]
        [SerializeField]
        /// <summary>
        /// 【目的】背景クリック時に再生する SE を指定する。
        /// 【理由】UI 操作フィードバックを統一し、UX を向上させるため。
        /// </summary>
        private SEType _clickSE = SEType.NESRPG0112;

#if UNITY_EDITOR
        /// <summary>
        /// 【目的】エディタ上で必要な参照を自動補完し、設定漏れを防ぐ。
        /// 【理由】Prefab を複製した際に参照が外れても自動で復旧できるようにするため。
        /// </summary>
        private void OnValidate()
        {
            _clickTrigger ??= GetComponent<ObservableEventTrigger>();
        }
#endif

        protected override void BindPresenter(IContainerBuilder builder)
        {
            builder.RegisterEntryPoint<Wiring<IDisplayBackground, DisplayBackgroundContext>>()
                .WithParameter(CreateContext);
        }

        protected override DisplayBackgroundContext CreateContext(IObjectResolver resolver)
        {
            var audioFacade = resolver.Resolve<IAudioFacade>();
            var binder = new ClickEventBinder<PointerEventData>(
                _clickTrigger.OnPointerClickAsObservable(),
                audioFacade,
                _clickSE
            );
            
            return new DisplayBackgroundContext
            {
                ClickEventBinder = binder,
                View = resolver.Resolve<IDisplayBackgroundView>()
            };
        }
    }
}
