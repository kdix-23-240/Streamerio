// ============================================================================
// モジュール概要: RectTransform の拡大縮小アニメーションを DOTween で提供し、UI 演出の統一を支援する。
// 外部依存: Cysharp.Threading.Tasks（UniTask）、DG.Tweening（DOTween）、UnityEngine。
// 使用例: Hover 演出の Presenter が ScaleAnimationComponent を生成し、フェードと組み合わせて表示効果を高める。
// ============================================================================

using Cysharp.Threading.Tasks;
using DG.Tweening;
using System.Threading;
using UnityEngine;

namespace Common.UI.Animation
{
    /// <summary>
    /// RectTransform のスケールを変化させるアニメーションコンポーネント。
    /// - DOTween の DOScale を利用
    /// - Duration / Ease / Scale はパラメータで制御
    /// - 非同期実行 & CancellationToken による中断が可能
    /// </summary>
    public class ScaleAnimationComponent : IUIAnimationComponent
    {
        private readonly RectTransform _rectTransform;
        private readonly ScaleAnimationComponentParamSO _param;

        /// <summary>
        /// 【目的】対象 RectTransform とパラメータを関連付け、再生時の参照取得コストをなくす。
        /// </summary>
        /// <param name="rectTransform">【用途】拡大縮小させたい UI の RectTransform。</param>
        /// <param name="param">【用途】拡大率やイージングを保持する ScriptableObject。</param>
        public ScaleAnimationComponent(RectTransform rectTransform, ScaleAnimationComponentParamSO param)
        {
            _rectTransform = rectTransform;
            _param = param;
        }
        
        /// <summary>
        /// スケールアニメーションを再生。
        /// - 指定された Scale 値まで拡縮
        /// - Duration と Ease はパラメータ依存
        /// </summary>
        public async UniTask PlayAsync(CancellationToken ct)
        {
            await _rectTransform
                .DOScale(_param.Scale, _param.DurationSec)
                .SetEase(_param.Ease)
                .ToUniTask(cancellationToken: ct);
        }
    }
}
