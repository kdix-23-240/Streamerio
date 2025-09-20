using System;
using System.Threading;
using Alchemy.Inspector;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

namespace Common.UI.Animation
{
    /// <summary>
    /// パスを通るアニメーション
    /// </summary>
    public class PathAnimationComponent: IUIAnimationComponent
    {
        private readonly RectTransform _rectTransform;
        private readonly PathAnimationComponentParam _param;

        public PathAnimationComponent(RectTransform rectTransform, PathAnimationComponentParam param)
        {
            _rectTransform = rectTransform;
            _param = param;
        }

        public async UniTask PlayAsync(CancellationToken ct)
        {
            await _rectTransform
                .DOLocalPath(_param.Path, _param.Duration, _param.PathType)
                .SetEase(_param.Ease)
                .ToUniTask(cancellationToken: ct);
        }  
    }

    [Serializable]
    public class PathAnimationComponentParam : UIAnimationComponentParam
    {
        [SerializeField, LabelText("オブジェクトが通る点(先頭から順に)")]
        public Vector3[] Path;
        [SerializeField, LabelText("パスの種類")]
        public PathType PathType = PathType.CatmullRom;
    }
}