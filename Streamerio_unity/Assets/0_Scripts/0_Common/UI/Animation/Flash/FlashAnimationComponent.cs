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
        public FlashAnimationComponent(CanvasGroup canvasGroup, FlashAnimationComponentParamSO param)
        {
            SetSequence(canvasGroup, param);
        }

        /// <summary>
        /// DOTween の Sequence を構築。
        /// - 各 CanvasGroup に対してフェードを追加
        /// - パーツ間にインターバルを挿入して「順番に」アニメーションする
        /// </summary>
        private void SetSequence(CanvasGroup canvasGroup, FlashAnimationComponentParamSO paramSo)
        {
            Sequence.Append(canvasGroup
                .DOFade(paramSo.MinAlpha, paramSo.DurationSec)
                .SetEase(paramSo.Ease));
            
            Sequence.Append(canvasGroup
                .DOFade(paramSo.MaxAlpha, paramSo.DurationSec)
                .SetEase(paramSo.Ease));
            
            // FlashCount: 1回 = フェードアウト+フェードイン
            Sequence.SetLoops(paramSo.FlashCount == -1 ? -1 : paramSo.FlashCount * 2, LoopType.Restart);
        }
    }
}