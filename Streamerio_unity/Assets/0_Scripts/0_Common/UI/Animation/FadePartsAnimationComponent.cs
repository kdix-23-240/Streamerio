using System;
using System.Threading;
using Alchemy.Inspector;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

namespace Common.UI.Animation
{
    public class FadePartsAnimationComponent: IUIAnimationComponent
    {
        private Sequence _sequence;
        
        public FadePartsAnimationComponent(CanvasGroup[] canvasGroups, FadePartsAnimationComponentParam param)
        {
            SetSequence(canvasGroups, param);
        }
        
        private void SetSequence(CanvasGroup[] canvasGroups, FadePartsAnimationComponentParam param)
        {
            _sequence = DOTween.Sequence().Pause();
            
            foreach (var canvasGroup in canvasGroups)
            {
                _sequence.Join(canvasGroup
                    .DOFade(param.Alpha, param.DurationSec)
                    .SetEase(param.Ease));
                _sequence.AppendInterval(param.ShowDelaySec);
            }
        }
        
        public async UniTask PlayAsync(CancellationToken ct)
        {
            _sequence.Restart();
            
            await _sequence.Play()
                .ToUniTask(cancellationToken: ct);
        }
    }

    [Serializable]
	public class FadePartsAnimationComponentParam : FadeAnimationComponentParam
    {
        [SerializeField, LabelText("パーツごとの表示ディレイ(秒)"), Min(0.001f)]
        public float ShowDelaySec = 0.05f;
    }
}