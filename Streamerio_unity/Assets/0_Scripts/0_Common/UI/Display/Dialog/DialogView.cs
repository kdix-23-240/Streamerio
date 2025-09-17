using Alchemy.Inspector;
using DG.Tweening;
using UnityEngine;

namespace Common.UI.Dialog
{
    public class DialogView : MonoBehaviour
    {
        [SerializeField, Alchemy.Inspector.ReadOnly]
        private CanvasGroup _canvasGroup;

        [Header("透明度")]
        [SerializeField, LabelText("表示時の透明度"), Range(0f, 1f)]
        private float _showAlpha = 1f;
        [SerializeField, LabelText("非表示時の透明度"), Range(0f, 1f)]
        private float _hideAlpha = 0f;

        [Header("イージング")]
        [SerializeField, LabelText("表示時のイージング")]
        private Ease _showEase = Ease.InSine;
        [SerializeField, LabelText("非表示時のイージング")]
        private Ease _hideEase = Ease.OutSine;
    
        [SerializeField, Min(0.01f), Alchemy.Inspector.LabelText("アニメーションの時間")]
        private float _animationDuration;
    }
}
