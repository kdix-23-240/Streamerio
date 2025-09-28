using Cysharp.Threading.Tasks;
using DG.Tweening;
using System;
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
        private readonly ScaleAnimationComponentParam _param;

        public ScaleAnimationComponent(RectTransform rectTransform, ScaleAnimationComponentParam param)
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
    
    /// <summary>
    /// スケールアニメーションの設定パラメータ。
    /// - Scale: 最終的に到達する拡縮率 (1 = 元のサイズ)
    /// - DurationSec: 補間時間
    /// - Ease: 補間方法
    /// </summary>
    [Serializable]
    public class ScaleAnimationComponentParam : UIAnimationComponentParam
    {
        [Header("最終的な大きさ (1 = 等倍)")]
        [SerializeField, Range(0f, 2f)]
        public float Scale = 1f;
    }
}