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
    /// 背景領域用の LifetimeScope。
    /// <para>
    /// - 背景 View／クリックトリガー／SE の依存をコンテナに登録  
    /// - 背景クリック時の挙動（例：閉じる処理・SE 再生など）を制御する Presenter を構築
    /// </para>
    /// </summary>
    [RequireComponent(typeof(ObservableEventTrigger), typeof(DisplayBackgroundView))]
    public class DisplayBackGroundLifetimeScope : LifetimeScope
    {
        [Header("References")]
        [SerializeField, ReadOnly]
        private DisplayBackgroundView _view;

        [SerializeField, ReadOnly]
        private ObservableEventTrigger _clickTrigger;

        [Header("Settings")]
        [Tooltip("背景クリック時に再生する SE")]
        [SerializeField]
        private SEType _clickSE = SEType.NESRPG0112;


#if UNITY_EDITOR
        /// <summary>
        /// エディタ上で参照を自動補完する。
        /// </summary>
        private void OnValidate()
        {
            _view ??= GetComponent<DisplayBackgroundView>();
            _clickTrigger ??= GetComponent<ObservableEventTrigger>();
        }
#endif

        /// <summary>
        /// コンテナに依存性を登録する。
        /// </summary>
        /// <remarks>
        /// - View はインスタンスを直接登録  
        /// - クリックトリガーと SE をパラメータとして ClickEventBinder を登録  
        /// - Presenter を Singleton として登録
        /// </remarks>
        protected override void Configure(IContainerBuilder builder)
        {
            // 背景 View の登録
            builder
                .RegisterComponent(_view)
                .As<IDisplayView>()
                .As<IInitializable>();

            // クリックイベントのバインダ登録（PointerEventData を利用）
            builder
                .Register<IClickEventBinder>(_ =>
                    new ClickEventBinder<PointerEventData>(
                        _clickTrigger.OnPointerClickAsObservable(),
                        _.Resolve<IAudioFacade>()
                    ),Lifetime.Singleton
                );

            // 背景 Presenter の登録
            builder
                .Register<DisplayBackgroundPresenter>(Lifetime.Singleton)
                .AsSelf()
                .As<IInitializable>();
        }
    }
}