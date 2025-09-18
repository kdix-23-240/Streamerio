using System;
using System.Threading;
using Alchemy.Inspector;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

namespace Common.UI.Animation
{
    /// <summary>
    /// 点滅アニメーション
    /// </summary>
    public class FlashAnimationComponent: IUIAnimationComponent
    {
        private readonly CanvasGroup _canvasGroup;
        private readonly FlashAnimationComponentParam _param;
        
        public FlashAnimationComponent(CanvasGroup canvasGroup, FlashAnimationComponentParam param)
        {
            _canvasGroup = canvasGroup;
            _param = param;
        }
        
        public async UniTask PlayAsync(CancellationToken ct)
        {
            var sequence = DOTween.Sequence();
            
            sequence.Append(_canvasGroup
                .DOFade(_param.MinAlpha, _param.Duration)
                .SetEase(_param.Ease));
            sequence.Append(_canvasGroup
                .DOFade(_param.MaxAlpha, _param.Duration)
                .SetEase(_param.Ease));
            sequence.SetLoops(_param.FlashCount == -1 ? -1 : _param.FlashCount * 2);
            await sequence.Play().ToUniTask(cancellationToken: ct);
        }
    }
    
    [Serializable]
    public class FlashAnimationComponentParam : UIAnimationComponentParam
    {
        [Header("透明度")]
        [SerializeField, LabelText("最小の透明度"), Range(0f, 1f)]
        public float MinAlpha = 0f;
        [SerializeField, LabelText("最大の透明度"), Range(0f, 1f)]
        public float MaxAlpha = 1f;
        
        [Header("点滅回数(消えて戻るまでで1回)(-1で無限)")]
        [SerializeField, Min(-1)]
        public int FlashCount;
    }
}