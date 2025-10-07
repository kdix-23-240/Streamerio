using DG.Tweening;
using UnityEngine;

namespace Common.UI.Animation
{
    /// <summary>
    /// 複数の UI パーツ(CanvasGroup)をまとめてフェードさせるアニメーションコンポーネント。
    /// - DOTween の Sequence を使って順番にアニメーションを構築
    /// - 各パーツ間にディレイを挿入可能
    /// </summary>
    public class FadePartsAnimationComponent : SequenceAnimationComponentBase
    {
        /// <summary>
        /// コンストラクタ。
        /// - 指定された CanvasGroup 配列とパラメータで Sequence を構築
        /// </summary>
        public FadePartsAnimationComponent(CanvasGroup[] canvasGroups, FadePartsAnimationComponentParamSO param)
        {
            SetSequence(canvasGroups, param);
        }

        /// <summary>
        /// DOTween の Sequence を構築。
        /// - 各 CanvasGroup に対してフェードを追加
        /// - パーツ間にインターバルを挿入して「順番に」アニメーションする
        /// </summary>
        private void SetSequence(CanvasGroup[] canvasGroups, FadePartsAnimationComponentParamSO param)
        {
            foreach (var canvasGroup in canvasGroups)
            {
                Sequence.Join(canvasGroup
                    .DOFade(param.Alpha, param.DurationSec)
                    .SetEase(param.Ease));
                Sequence.AppendInterval(param.ShowDelaySec);
            }
        }
    }
}
