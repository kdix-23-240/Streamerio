using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

namespace Common.UI.Animation
{
    /// <summary>
    /// 移動先の位置まで移動するアニメーション
    /// </summary>
    public class MoveAnimationComponent: IUIAnimationComponent
    {
        private readonly RectTransform _rectTransform;
        private readonly MoveAnimationComponentParam _param;
        
        public MoveAnimationComponent(RectTransform rectTransform, MoveAnimationComponentParam param)
        {
            _rectTransform = rectTransform;
            _param = param;
        }
        
        public async UniTask PlayAsync(CancellationToken ct)
        {
            await _rectTransform
                .DOAnchorPos(_param.Position, _param.Duration)
                .SetEase(_param.Ease)
                .ToUniTask(cancellationToken: ct);
        }
    }
    
    [Serializable]
    public class MoveAnimationComponentParam : UIAnimationComponentParam
    {
        [Header("移動先")]
        [SerializeField]
        public Vector2 Position;
    }
}