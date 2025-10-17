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
    [RequireComponent(typeof(ObservableEventTrigger), typeof(DisplayBackgroundView))]
    public class DisplayBackGroundLifetimeScope : LifetimeScope
    {
        [Header("References")]
        /// <summary>
        /// 【目的】背景 View コンポーネントを Inspector から参照し、DI 登録へ利用する。
        /// 【理由】LifetimeScope が生成された瞬間に必須参照が埋まっていることを保証するため。
        /// </summary>
        [SerializeField, ReadOnly]
        private DisplayBackgroundView _view;

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
            _view ??= GetComponent<DisplayBackgroundView>();
            _clickTrigger ??= GetComponent<ObservableEventTrigger>();
        }
#endif

        /// <summary>
        /// 【目的】背景 UI の依存を DI コンテナへ登録する。
        /// 【処理概要】
        ///   1. View を IDisplayBackgroundView / IInitializable として登録。
        ///   2. ClickEventBinder を生成し、Wiring 経由で Presenter に渡す。
        ///   3. AudioFacade を利用してクリック時の SE 再生を組み込む。
        /// 【理由】背景モジュールを単独で生成しても、Presenter が必要な依存を確実に得られるようにするため。
        /// </summary>
        /// <param name="builder">【用途】依存登録を構築する VContainer のビルダー。</param>
        protected override void Configure(IContainerBuilder builder)
        {
            builder
                .RegisterComponent(_view)
                .As<IDisplayBackgroundView>()
                .As<IInitializable>();

            builder
                .RegisterEntryPoint<Wiring<DisplayBackgroundPresenter, DisplayBackgroundContext>>()
                .WithParameter(resolver =>
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
                        View = _view
                    };
                });
        }
    }
}
