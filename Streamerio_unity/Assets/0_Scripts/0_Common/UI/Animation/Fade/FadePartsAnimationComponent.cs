// ============================================================================
// モジュール概要: 複数 CanvasGroup を連続フェードさせる DOTween Sequence コンポーネントを提供する。
// 外部依存: DG.Tweening（DOTween）、UnityEngine。
// 使用例: チュートリアル UI の段階的表示などで FadePartsAnimationComponent を利用し、パーツ単位の演出を統一する。
// ============================================================================

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
        /// 【目的】対象パーツ群とパラメータを受け取り、再利用可能な DOTween Sequence を組み立てる。
        /// 【理由】コンストラクタ内でセットアップしておくことで、PlayAsync 呼び出し時に余計な GC を発生させないため。
        /// </summary>
        public FadePartsAnimationComponent(CanvasGroup[] canvasGroups, FadePartsAnimationComponentParamSO param)
        {
            SetSequence(canvasGroups, param);
        }

        /// <summary>
        /// 【目的】DOTween の Sequence を構築し、遅延込みで順次フェードする動きを定義する。
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
