using Cysharp.Threading.Tasks;
using DG.Tweening;
using System;
using System.Threading;
using UnityEngine;

namespace Common.UI.Animation
{
    
    /// <summary>
    /// UIのフェードアニメーション
    /// </summary>
    public class FadeAnimationComponent: IUIAnimationComponent
    {
        private readonly CanvasGroup _canvasGroup;

        private readonly FadeAnimationComponentParam _param;
        public FadeAnimationComponent(CanvasGroup canvasGroup, FadeAnimationComponentParam param)
        {
            _canvasGroup = canvasGroup;
            _param = param;
        }
        
        public async UniTask PlayAsync(CancellationToken ct)
        {
            await _canvasGroup
                .DOFade(_param.Alpha, _param.Duration)
                .SetEase(_param.Ease)
                .ToUniTask(cancellationToken: ct);
        }
    }

    [Serializable]
    public class FadeAnimationComponentParam : UIAnimationComponentParam
    {
        [Header("透明度")]
        [SerializeField, Range(0f, 1f)]
        public float Alpha;
    }
}