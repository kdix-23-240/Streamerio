using Alchemy.Inspector;
using DG.Tweening;
using UnityEngine;

namespace Common.UI.Animation
{
    /// <summary>
    /// パス移動アニメーションの設定パラメータ。
    /// - Path: オブジェクトが通る座標リスト（ローカル座標系）
    /// - PathType: 補間方法（Linear / CatmullRom など）
    /// - DurationSec / Ease: アニメーション速度やイージング（基底から継承）
    /// </summary>
    [CreateAssetMenu(fileName = "PathAnimationSO", menuName = "SO/UI/Animation/Path")]
    public class PathAnimationComponentParamSO : UIAnimationComponentParamSO
    {
        [SerializeField, LabelText("オブジェクトが通る点 (先頭から順に)")]
        public Vector3[] Path;

        [SerializeField, LabelText("パスの種類 (直線/曲線)")]
        public PathType PathType = PathType.CatmullRom;
    }
}