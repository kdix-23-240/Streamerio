using Alchemy.Inspector;
using UnityEngine;

namespace Common.UI.Animation
{
    /// <summary>
    /// 点滅アニメーションの設定パラメータ。
    /// - MinAlpha: 最小の透明度
    /// - MaxAlpha: 最大の透明度
    /// - FlashCount: 点滅回数（-1 なら無限）
    /// </summary>
    [CreateAssetMenu(fileName = "FlashAnimationSO", menuName = "SO/UI/Animation/Flash")]
    public class FlashAnimationComponentParamSO : UIAnimationComponentParamSO
    {
        [Header("透明度設定")]
        [SerializeField, LabelText("最小の透明度"), Range(0f, 1f)]
        public float MinAlpha = 0f;
        
        [SerializeField, LabelText("最大の透明度"), Range(0f, 1f)]
        public float MaxAlpha = 1f;
        
        [Header("点滅回数 (消えて戻るまでで1回) (-1で無限)")]
        [SerializeField, Min(-1)]
        public int FlashCount;
    }
}