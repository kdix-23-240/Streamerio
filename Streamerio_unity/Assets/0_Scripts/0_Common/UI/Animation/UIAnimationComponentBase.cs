using Alchemy.Inspector;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using System;
using System.Threading;
using UnityEngine;

namespace Common.UI.Animation
{
    /// <summary>
    /// UIのアニメーションの基底クラス
    /// </summary>
    public interface IUIAnimationComponent
    {
        /// <summary>
        /// アニメーションを再生
        /// </summary>
        /// <param name="ct"></param>
        /// <returns></returns>
        UniTask PlayAsync(CancellationToken ct);
    }

    /// <summary>
    /// UIアニメーションの共通パラメータ
    /// </summary>
    [Serializable]
    public class UIAnimationComponentParam
    {
        [Header("アニメーションの時間")]
        [SerializeField, Min(0.01f)]
        public float Duration;

        [Header("イージング")]
        [SerializeField]
        public Ease Ease;
    }
}