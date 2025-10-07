using Alchemy.Inspector;
using UnityEngine;

namespace Common.UI.Animation
{
    /// <summary>
    /// Irisアニメーションのパラメータ。
    /// - 中心座標
    /// - 半径プロパティ名
    /// - 開閉範囲（Min/Max半径）
    /// </summary>
    [CreateAssetMenu(fileName = "IrisAnimationSO", menuName = "SO/UI/Animation/Iris")]
    public class IrisAnimationComponentParamSO : UIAnimationComponentParamSO
    {
        [SerializeField, LabelText("円の中心のプロパティ名")]
        public string CenterPropertyName = "_CenterUV";
        
        [SerializeField, LabelText("円の半径のプロパティ名")]
        public string RadiusPropertyName = "_Radius";
        
        [SerializeField, LabelText("円の中心位置 (UV座標)")]
        public Vector2 Center = new(0.5f, 0.5f);
        
        [SerializeField, LabelText("イリスの最小半径")]
        public float MinRadius = 0f;
        
        [SerializeField, LabelText("イリスの最大半径")]
        public float MaxRadius = 1.5f;
    }
}