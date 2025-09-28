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
    public class FlashAnimationComponent : IUIAnimationComponent
    {
        private readonly CanvasGroup _canvasGroup;
        private readonly FlashAnimationComponentParam _param;
        
        public FlashAnimationComponent(CanvasGroup canvasGroup, FlashAnimationComponentParam param)
        {
            _canvasGroup = canvasGroup;
            _param = param;
        }
        
        /// <summary>
        /// 点滅アニメーションを再生。
        /// - MinAlpha → MaxAlpha を交互に補間
        /// - FlashCount に応じて繰り返し回数を制御（-1 なら無限）
        /// - CancellationToken で途中停止可能
        /// </summary>
        public async UniTask PlayAsync(CancellationToken ct)
        {
            var sequence = DOTween.Sequence();
            
            sequence.Append(_canvasGroup
                .DOFade(_param.MinAlpha, _param.DurationSec)
                .SetEase(_param.Ease));
            
            sequence.Append(_canvasGroup
                .DOFade(_param.MaxAlpha, _param.DurationSec)
                .SetEase(_param.Ease));
            
            // FlashCount: 1回 = フェードアウト+フェードイン
            sequence.SetLoops(_param.FlashCount == -1 ? -1 : _param.FlashCount * 2);
            
            await sequence.Play().ToUniTask(cancellationToken: ct);
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