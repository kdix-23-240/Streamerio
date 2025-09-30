using System;
using System.Threading;
using Alchemy.Inspector;
using Cysharp.Threading.Tasks;
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
        public FlashAnimationComponent(CanvasGroup canvasGroup, FlashAnimationComponentParam param): base()
        {
            SetSequence(canvasGroup, param);
        }

        /// <summary>
        /// DOTween の Sequence を構築。
        /// - 各 CanvasGroup に対してフェードを追加
        /// - パーツ間にインターバルを挿入して「順番に」アニメーションする
        /// </summary>
        private void SetSequence(CanvasGroup canvasGroup, FlashAnimationComponentParam param)
        {
            Sequence.Append(canvasGroup
                .DOFade(param.MinAlpha, param.DurationSec)
                .SetEase(param.Ease));
            
            Sequence.Append(canvasGroup
                .DOFade(param.MaxAlpha, param.DurationSec)
                .SetEase(param.Ease));
            
            // FlashCount: 1回 = フェードアウト+フェードイン
            Sequence.SetLoops(param.FlashCount == -1 ? -1 : param.FlashCount * 2, LoopType.Restart);
        }
    }
    
    /// <summary>
    /// 点滅アニメーションの設定パラメータ。
    /// - MinAlpha: 最小の透明度
    /// - MaxAlpha: 最大の透明度
    /// - FlashCount: 点滅回数（-1 なら無限）
    /// </summary>
    [Serializable]
    public class FlashAnimationComponentParam : UIAnimationComponentParam
    {
        [Header("透明度設定")]
        [SerializeField, LabelText("最小の透明度"), Range(0f, 1f)]
        public float MinAlpha = 0f;
        
        [SerializeField, LabelText("最大の透明度"), Range(0f, 1f)]
        public float MaxAlpha = 1f;
        
        [Header("点滅回数 (消えて戻るまでで1回) (-1で無限)")]
        [SerializeField, Min(-1)]
        public int FlashCount;
    }
}