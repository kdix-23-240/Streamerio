using UnityEngine;

namespace Common.UI.Animation
{
    /// <summary>
    /// スケールアニメーションの設定パラメータ。
    /// - Scale: 最終的に到達する拡縮率 (1 = 元のサイズ)
    /// - DurationSec: 補間時間
    /// - Ease: 補間方法
    /// </summary>
    [CreateAssetMenu(fileName = "ScaleAnimationSO", menuName = "SO/UI/Animation/Scale")]
    public class ScaleAnimationComponentParamSO : UIAnimationComponentParamSO
    {
        [Header("最終的な大きさ (1 = 等倍)")]
        [SerializeField, Range(0f, 2f)]
        public float Scale = 1f;
    }
}