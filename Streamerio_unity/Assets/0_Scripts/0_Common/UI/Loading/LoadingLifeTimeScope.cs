// ============================================================================
// モジュール概要: ローディング画面の依存解決スコープを構築し、View と Presenter を VContainer で接続する。
// 外部依存: VContainer（DI フレームワーク）。
// 使用例: ローディングシーンで LoadingLifeTimeScope を配置し、ILoadingScreenView を自動解決する。
// ============================================================================

using VContainer;
using VContainer.Unity;

namespace Common.UI.Loading
{
    /// <summary>
    /// ローディング UI 専用の LifetimeScope。
    /// <para>
    /// 【理由】View コンポーネントと Presenter を DI で結び、シーン遷移時にも同一設定を再利用できるようにする。
    /// </para>
    /// </summary>
    public class LoadingLifeTimeScope: LifetimeScope
    {
        /// <summary>
        /// 【目的】ローディング画面に必要な依存をコンテナへ登録する。
        /// <para>
        /// - View を ILoadingScreenView / IInitializable として登録し、外部から解決できるようにする。<br/>
        /// - Wiring を EntryPoint として登録し、Presenter 初期化のタイミングを制御する。
        /// </para>
        /// </summary>
        protected override void Configure(IContainerBuilder builder)
        {
            // シーン上に存在する View インスタンスをそのまま DI へ登録するため、GetComponent で取得する。
            var view = GetComponent<ILoadingScreenView>();
            builder.RegisterComponent(view)
                .As<ILoadingScreenView>()
                .As<IInitializable>();

            builder
                .RegisterEntryPoint<Wiring<ILoadingScreen, LoadingScreenPresenterContext>>()
                .WithParameter(resolver =>
                {
                    // PresenterContext に View を手動注入することで、Prefab に組み込まれた View を再利用しつつ DI を完結させる。
                    return new LoadingScreenPresenterContext
                    {
                        View = resolver.Resolve<ILoadingScreenView>()
                    };
                });
        }
    }
}
