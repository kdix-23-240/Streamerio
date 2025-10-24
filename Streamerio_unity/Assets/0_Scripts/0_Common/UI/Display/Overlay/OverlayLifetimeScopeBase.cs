// ============================================================================
// モジュール概要: オーバーレイ UI の LifetimeScope 基底クラスを提供し、共通部品グループの依存登録を統一する。
// 外部依存: VContainer、Common.UI.Part.Group。
// 使用例: 特定オーバーレイ用に派生させ、共通パーツグループを DI で提供して演出制御を共通化する。
// ============================================================================

using Common.UI.Part.Group;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Common.UI.Display.Overlay
{
    /// <summary>
    /// 【目的】オーバーレイ UI の LifetimeScope を構築する際の共通処理をまとめる基底クラス。
    /// 【理由】各オーバーレイで部品グループの登録を繰り返さず、DI 設定の重複を防ぐ。
    /// </summary>
    public abstract class OverlayLifetimeScopeBase<TOverlay, TPresenter, TView, TContext>: DisplayLifetimeScopeBase<TOverlay, TPresenter, TView, TContext>
        where TOverlay : IOverlay, IAttachable<TContext>
        where TPresenter : TOverlay, IStartable
        where TView : IOverlayView
        where TContext: CommonOverlayContext<TView>
    {
        /// <summary>
        /// 【目的】パーツ表示演出を司るコンポーネントを Inspector から受け取り、DI で共有する。
        /// </summary>
        [SerializeField]
        [Tooltip("Overlay 表示時に順次表示させる共通パーツグループ。")]
        private CommonUIPartGroup _partGroup;
        
        /// <summary>
        /// 【目的】パーツグループを DI コンテナへ登録し、基底 Configure の登録と組み合わせる。
        /// 【理由】Overlay 毎に共通パーツ制御を利用できるよう、基底で登録しておく。
        /// </summary>
        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterComponent(_partGroup)
                .As<ICommonUIPartGroup>()
                .As<IInitializable>();
            
            base.Configure(builder);
        }
    }
}
