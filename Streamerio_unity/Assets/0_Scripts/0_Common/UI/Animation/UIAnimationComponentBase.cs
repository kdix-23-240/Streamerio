using Cysharp.Threading.Tasks;
using DG.Tweening;
using System;
using System.Threading;
using UnityEngine;
using UnityEngine.Serialization;

namespace Common.UI.Animation
{
    /// <summary>
    /// UI アニメーションの基底インターフェース。
    /// - 各種 UI アニメーションコンポーネントが実装する
    /// - 非同期でアニメーションを実行し、キャンセル対応も可能
    /// </summary>
    public interface IUIAnimationComponent
    {
        /// <summary>
        /// アニメーションを再生する。
        /// - UniTask を返すので await 可能
        /// - CancellationToken による中断に対応
        /// </summary>
        UniTask PlayAsync(CancellationToken ct);
    }

    /// <summary>
    /// UI アニメーションの共通パラメータ。
    /// - 再生時間
    /// - イージング
    /// などの基本的な設定を保持
    /// </summary>
    [Serializable]
    public class UIAnimationComponentParam
    {
        [FormerlySerializedAs("Duration")]
        [Header("アニメーションの時間(秒)")]
        [SerializeField, Min(0.01f)]
        public float DurationSec;

        [Header("イージング種類")]
        [SerializeField]
        public Ease Ease = Ease.Linear;
    }
}