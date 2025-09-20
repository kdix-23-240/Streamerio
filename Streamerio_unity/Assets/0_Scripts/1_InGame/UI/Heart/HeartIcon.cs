using Alchemy.Inspector;
using Common.UI;
using UnityEngine;
using UnityEngine.UI;

namespace InGame.UI.Heart
{
    /// <summary>
    /// ハートアイコン
    /// </summary>
    public class HeartIcon: UIBehaviourBase
    {
        public const float MinValue = 0;
        public const float MaxValue = 1;
        
        [SerializeField, LabelText("ハートのアイコン")]
	    private Image _heartIcon;

        /// <summary>
        /// ハートの値を設定(1以上で満タン、0以下で空)
        /// </summary>
        /// <param name="value"></param>
        public void SetValue(float value)
        {
            _heartIcon.fillAmount = Mathf.Clamp(value, MinValue, MaxValue);
        }
    }
}