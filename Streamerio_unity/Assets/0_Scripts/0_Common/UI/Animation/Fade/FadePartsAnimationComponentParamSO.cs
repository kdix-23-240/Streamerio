using Alchemy.Inspector;
using UnityEngine;

namespace Common.UI.Animation
{
    /// <summary>
    /// FadePartsAnimationComponent の設定用パラメータ。
    /// - Alpha / Duration / Ease は FadeAnimationComponentParam から継承
    /// - ShowDelaySec を追加してパーツごとの遅延を指定可能
    /// </summary>
    [CreateAssetMenu(fileName = "FadePartsAnimationSO", menuName = "SO/UI/Animation/FadeParts")]
    public class FadePartsAnimationComponentParamSO : FadeAnimationComponentParamSO
    {
        [SerializeField, LabelText("パーツごとの表示ディレイ(秒)"), Min(0.001f)]
        public float ShowDelaySec = 0.05f;
    }
}