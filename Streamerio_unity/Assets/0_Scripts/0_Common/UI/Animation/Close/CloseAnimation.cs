using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

namespace Common.UI.Animation
{
    public class CloseAnimation: SequenceAnimationBase
    {
        private readonly RectTransform _rectTransform;
        private readonly CloseAnimationParamSO _param;
        
        public CloseAnimation(RectTransform rectTransform, CloseAnimationParamSO param)
        {
            _rectTransform = rectTransform;
            _param = param;

            SetSequence();
        }

        public override async UniTask PlayAsync(CancellationToken ct, bool useInitial = true)
        {
            if (useInitial)
            {
                _rectTransform.anchoredPosition = _param.InitialAnchoredPosition;
                _rectTransform.sizeDelta = _param.InitialScale;
            }
            
            await base.PlayAsync(ct, useInitial);
        }

        public override void PlayImmediate()
        {
            _rectTransform.anchoredPosition = _param.AnchoredPosition;
            _rectTransform.sizeDelta = _param.Scale;
        }

        private void SetSequence()
        {
            Sequence.Append(_rectTransform
                .DOAnchorPos(_param.AnchoredPosition, _param.DurationSec)
                .SetEase(_param.Ease));
            
            Sequence.Join(_rectTransform
                .DOSizeDelta(_param.Scale, _param.DurationSec)
                .SetEase(_param.Ease));
        }
    }
}