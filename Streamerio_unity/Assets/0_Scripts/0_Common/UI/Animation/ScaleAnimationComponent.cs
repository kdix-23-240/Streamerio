using Cysharp.Threading.Tasks;
using DG.Tweening;
using System;
using System.Threading;
using UnityEngine;

namespace Common.UI.Animation
{
    /// <summary>
    /// 大きさを変えるアニメーション
    /// </summary>
    public class ScaleAnimationComponent: IUIAnimationComponent
    {
        private RectTransform _rectTransform;
        private ScaleAnimationComponentParam _param;

        public ScaleAnimationComponent(RectTransform rectTransform, ScaleAnimationComponentParam param)
        {
            _rectTransform = rectTransform;
            _param = param;
        }
        
        public async UniTask PlayAsync(CancellationToken ct)
        {
            await _rectTransform
                .DOScale(_param.Scale, _param.Duration)
                .SetEase(_param.Ease)
                .ToUniTask(cancellationToken: ct);
        }
    }
    
    [Serializable]
    public class ScaleAnimationComponentParam : UIAnimationComponentParam
    {
        [Header("大きさ")]
        [SerializeField, Range(0f, 1f)]
        public float Scale;
    }
}