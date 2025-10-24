// ============================================================================
// モジュール概要: Overlay UI の基底 View 実装を提供し、フェード演出やパーツ制御を共通化する。
// 外部依存: Alchemy.Inspector、Cysharp.Threading.Tasks、Common.UI.Animation、VContainer。
// 使用例: OverlayViewBase を継承して固有演出を追加しつつ、共通のフェード/パーツ制御を再利用する。
// ============================================================================

using Alchemy.Inspector;
using Common.UI.Animation;
using Cysharp.Threading.Tasks;
using System.Threading;
using Common.UI.Part.Group;
using UnityEngine;
using VContainer;

namespace Common.UI.Display.Overlay
{
    /// <summary>
    /// オーバーレイ UI の View 契約。
    /// <para>
    /// 【理由】Presenter が OverlayViewBase に依存しなくても操作できるよう抽象化する。
    /// </para>
    /// </summary>
    public interface IOverlayView : IDisplayView
    {
    }
    
    /// <summary>
    /// Overlay 系 UI の見た目をフェードアニメーションで制御する基底クラス。
    /// - 複数のパーツを順番にフェード表示
    /// - 非表示は CanvasGroup をまとめてフェード
    /// - 即時表示/非表示も可能
    /// </summary>
    public abstract class OverlayViewBase : DisplayViewBase, IOverlayView
    {
        // 部分的なフェード演出を委譲するグループ。Overlay 表示時に順次表示する役割を担う。
        private ICommonUIPartGroup _partGroup;
        
        [Header("アニメーション")] 
        [SerializeField, LabelText("非表示アニメーション")]
        [Tooltip("オーバーレイを非表示にするときのフェード設定。")]
        private FadeAnimationComponentParamSO _hideAnimationParam;
        
        private FadeAnimationComponent _hideAnimation;

        [Inject]
        /// <summary>
        /// 【目的】DI から共通パーツグループを受け取り、表示/非表示のアニメーションを委譲する。
        /// </summary>
        public virtual void Construct(ICommonUIPartGroup partGroup)
        {
            _partGroup = partGroup;
        }

        public override void Initialize()
        {
            base.Initialize();
            
            _hideAnimation = new (CanvasGroup, _hideAnimationParam);
        }
        
        /// <summary>
        /// アニメーションで表示。
        /// - CanvasGroup を表示状態に設定
        /// - UI パーツを順番にフェードイン
        /// </summary>
        public override async UniTask ShowAsync(CancellationToken ct)
        {
            CanvasGroup.alpha = UIUtil.DEFAULT_SHOW_ALPHA;

            await _partGroup.ShowAsync(ct);
        }
        
        /// <summary>
        /// 即時表示。
        /// - CanvasGroup を表示状態に設定
        /// - 全 UI パーツの透明度を一括変更
        /// </summary>
        public override void Show()
        {
            CanvasGroup.alpha = UIUtil.DEFAULT_SHOW_ALPHA;
            
            _partGroup.Show();
        }
        
        /// <summary>
        /// アニメーションで非表示。
        /// - 全体 CanvasGroup をフェードアウト
        /// - パーツの透明度も最終値に揃える
        /// </summary>
        public override async UniTask HideAsync(CancellationToken ct)
        {
            await _hideAnimation.PlayAsync(ct);
            
            _partGroup.Hide();
        }

        /// <summary>
        /// 即時非表示。
        /// - CanvasGroup を非表示状態に設定
        /// - 全パーツの透明度を一括変更
        /// </summary>
        public override void Hide()
        {
            CanvasGroup.alpha = _hideAnimationParam.Alpha;
            
            _partGroup.Hide();
        }
    }
}
