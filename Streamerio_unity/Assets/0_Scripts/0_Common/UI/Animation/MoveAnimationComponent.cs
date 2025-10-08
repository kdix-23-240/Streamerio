using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

namespace Common.UI.Animation
{
    /// <summary>
    /// RectTransform を指定した座標まで移動させるアニメーションコンポーネント。
    /// - DOTween の DOAnchorPos を利用
    /// - 非同期で再生でき、CancellationToken による中断も可能
    /// </summary>
    public class MoveAnimationComponent : IUIAnimationComponent
    {
        private readonly RectTransform _rectTransform;
        private readonly MoveAnimationComponentParam _param;
        
        public MoveAnimationComponent(RectTransform rectTransform, MoveAnimationComponentParam param)
        {
            _rectTransform = rectTransform;
            _param = param;
        }
        
        /// <summary>
        /// 移動アニメーションを再生する。
        /// - 目標座標へ補間移動
        /// - Duration と Ease はパラメータで制御
        /// </summary>
        public async UniTask PlayAsync(CancellationToken ct)
        {
            await _rectTransform
                .DOAnchorPos(_param.AnchoredPosition, _param.DurationSec)
                .SetEase(_param.Ease)
                .ToUniTask(cancellationToken: ct);
        }
    }
    
    /// <summary>
    /// 移動アニメーションの設定パラメータ。
    /// - Position: 目標座標（AnchoredPosition）
    /// - DurationSec: アニメーション時間（基底から継承）
    /// - Ease: 補間方法（基底から継承）
    /// </summary>
    [Serializable]
    public class MoveAnimationComponentParam : UIAnimationComponentParam
    {
        [Header("移動先座標")]
        [SerializeField]
        public Vector2 AnchoredPosition;
    }
}