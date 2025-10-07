using UnityEngine;

namespace Common.UI.Animation
{
    /// <summary>
    /// 移動アニメーションの設定パラメータ。
    /// - Position: 目標座標（AnchoredPosition）
    /// - DurationSec: アニメーション時間（基底から継承）
    /// - Ease: 補間方法（基底から継承）
    /// </summary>
    [CreateAssetMenu(fileName = "MoveAnimationSO", menuName = "SO/UI/Animation/Move")]
    public class MoveAnimationComponentParamSO : UIAnimationComponentParamSO
    {
        [Header("移動先座標")]
        [SerializeField]
        public Vector2 AnchoredPosition;
    }
}