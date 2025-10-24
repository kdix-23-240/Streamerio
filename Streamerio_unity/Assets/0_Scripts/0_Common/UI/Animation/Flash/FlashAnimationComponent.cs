// ============================================================================
// モジュール概要: CanvasGroup を点滅させるアニメーションを DOTween Sequence で構築し、注意喚起演出を簡潔に提供する。
// 外部依存: DG.Tweening、UnityEngine。
// 使用例: FlashAnimationComponent をバナーの強調表示に組み込み、フェード点滅を共通処理として使い回す。
// ============================================================================

using DG.Tweening;
using UnityEngine;

namespace Common.UI.Animation
{
    /// <summary>
    /// 点滅アニメーションコンポーネント。
    /// - CanvasGroup の alpha を補間してフェードイン/フェードアウトを繰り返す
    /// - DOTween のシーケンスで制御
    /// </summary>
    public class FlashAnimationComponent : SequenceAnimationComponentBase
    {
        /// <summary>
        /// 【目的】点滅対象とパラメータを受け取り、即座に再生可能な Sequence を構築する。
        /// 【理由】PlayAsync 呼び出し前にシーケンスを準備し、実行時 GC を避けるため。
        /// </summary>
        public FlashAnimationComponent(CanvasGroup canvasGroup, FlashAnimationComponentParamSO param)
        {
            SetSequence(canvasGroup, param);
        }

        /// <summary>
        /// 【目的】DOTween の Sequence を構築し、フェードアウト→フェードインを繰り返す動きを定義する。
        /// </summary>
        private void SetSequence(CanvasGroup canvasGroup, FlashAnimationComponentParamSO paramSo)
        {
            Sequence.Append(canvasGroup
                .DOFade(paramSo.MinAlpha, paramSo.DurationSec)
                .SetEase(paramSo.Ease));
            
            Sequence.Append(canvasGroup
                .DOFade(paramSo.MaxAlpha, paramSo.DurationSec)
                .SetEase(paramSo.Ease));
            
            // FlashCount はフェードアウト→フェードインを 1 契機としてカウントするため、DOTween のループ回数とは 2 倍の関係になる。
            Sequence.SetLoops(paramSo.FlashCount == -1 ? -1 : paramSo.FlashCount * 2, LoopType.Restart);
        }
    }
}
