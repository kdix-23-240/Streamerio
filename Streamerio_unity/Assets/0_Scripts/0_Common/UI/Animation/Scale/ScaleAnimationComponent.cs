using Cysharp.Threading.Tasks;
using DG.Tweening;
using System.Threading;
using UnityEngine;

namespace Common.UI.Animation
{
    /// <summary>
    /// RectTransform のスケールを変化させるアニメーションコンポーネント。
    /// - DOTween の DOScale を利用
    /// - Duration / Ease / Scale はパラメータで制御
    /// - 非同期実行 & CancellationToken による中断が可能
    /// </summary>
    public class ScaleAnimationComponent : IUIAnimationComponent
    {
        private readonly RectTransform _rectTransform;
        private readonly ScaleAnimationComponentParamSO _param;

        public ScaleAnimationComponent(RectTransform rectTransform, ScaleAnimationComponentParamSO param)
        {
            _rectTransform = rectTransform;
            _param = param;
        }
        
        /// <summary>
        /// スケールアニメーションを再生。
        /// - 指定された Scale 値まで拡縮
        /// - Duration と Ease はパラメータ依存
        /// </summary>
        public async UniTask PlayAsync(CancellationToken ct)
        {
            await _rectTransform
                .DOScale(_param.Scale, _param.DurationSec)
                .SetEase(_param.Ease)
                .ToUniTask(cancellationToken: ct);
        }
    }
}