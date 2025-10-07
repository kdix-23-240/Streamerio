using UnityEngine;

namespace Common.UI.Animation
{
    /// <summary>
    /// フェードアニメーションの設定パラメータ。
    /// - Alpha: 目標の透明度
    /// - DurationSec: アニメーション時間
    /// - Ease: イージング
    /// </summary>
    [CreateAssetMenu(fileName = "FadeAnimationSO", menuName = "SO/UI/Animation/Fade")]
    public class FadeAnimationComponentParamSO : UIAnimationComponentParamSO
    {
        [Header("透明度")]
        [SerializeField, Range(0f, 1f)]
        public float Alpha;
    }
}