// モジュール概要:
// UI 背景のフェード演出を担当する View 実装。Presenter から呼び出され、見た目の制御を集約する。
// 依存関係: FadeAnimationComponentParamSO で演出パラメータを受け取り、CanvasGroup を直接操作する。

using System.Threading;
using Common.UI.Animation;
using Cysharp.Threading.Tasks;
using UnityEngine;
using VContainer.Unity;

namespace Common.UI.Display.Background
{
    /// <summary>
    /// 【目的】背景 View の専用インターフェースを表すマーカー。
    /// </summary>
    public interface IDisplayBackgroundView : IDisplayView { }
    
    /// <summary>
    /// 【目的】背景のフェードイン/アウト演出を実装する View。
    /// 【理由】Presenter 側が演出詳細を意識せず、見た目制御のみを委譲できるようにするため。
    /// </summary>
    public class DisplayBackgroundView : DisplayViewBase, IInitializable, IDisplayBackgroundView
    {
        /// <summary>
        /// 【目的】フェードイン演出に使用するパラメータを Inspector から受け取る。
        /// 【理由】演出速度やイージングをデザイナーが調整できるようにするため。
        /// </summary>
        [SerializeField]
        private FadeAnimationComponentParamSO _showAnimationParam;

        /// <summary>
        /// 【目的】フェードアウト演出に使用するパラメータを保持する。
        /// 【理由】非表示時も印象を揃えられるよう、表示時とは別の設定を渡せるようにする。
        /// </summary>
        [SerializeField]
        private FadeAnimationComponentParamSO _hideAnimationParam;

        /// <summary>
        /// 【目的】表示演出を再生するアニメーションコンポーネントをキャッシュする。
        /// 【理由】毎回インスタンス生成すると GC が発生し、演出開始が遅れるため。
        /// </summary>
        private IUIAnimationComponent _showAnimation;
        /// <summary>
        /// 【目的】非表示演出を再生するアニメーションコンポーネントをキャッシュする。
        /// 【理由】演出切り替えが高速に行えるよう、初期化時に生成しておく。
        /// </summary>
        private IUIAnimationComponent _hideAnimation;

        /// <summary>
        /// 【目的】演出パラメータからフェードアニメーションを構築する。
        /// 【理由】Presenter の Attach 前にアニメーション準備を完了させ、初回表示での割り当てコストを抑えるため。
        /// </summary>
        public void Initialize()
        {
            _showAnimation = new FadeAnimationComponent(CanvasGroup, _showAnimationParam);
            _hideAnimation = new FadeAnimationComponent(CanvasGroup, _hideAnimationParam);
        }

        /// <inheritdoc />
        public override async UniTask ShowAsync(CancellationToken ct)
        {
            await _showAnimation.PlayAsync(ct);
        }

        /// <inheritdoc />
        public override void Show()
        {
            CanvasGroup.alpha = UIUtil.DEFAULT_SHOW_ALPHA;
        }

        /// <inheritdoc />
        public override async UniTask HideAsync(CancellationToken ct)
        {
            await _hideAnimation.PlayAsync(ct);
        }

        /// <inheritdoc />
        public override void Hide()
        {
            CanvasGroup.alpha = UIUtil.DEFAULT_HIDE_ALPHA;
        }
    }
}
