using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

namespace Common.UI.Animation
{
    /// <summary>
    /// UI のフェードアニメーションコンポーネント。
    /// - CanvasGroup の alpha を補間して透明度を変更
    /// - DOTween を利用して非同期でアニメーションを実行
    /// </summary>
    public class FadeAnimationComponent : IUIAnimationComponent
    {
        private readonly CanvasGroup _canvasGroup;
        private readonly FadeAnimationComponentParamSO _param;

        public FadeAnimationComponent(CanvasGroup canvasGroup, FadeAnimationComponentParamSO param)
        {
            _canvasGroup = canvasGroup;
            _param = param;
        }
        
        /// <summary>
        /// フェードアニメーションを再生。
        /// - 指定の Alpha 値まで補間
        /// - Duration と Ease はパラメータで制御
        /// - CancellationToken により中断可能
        /// </summary>
        public async UniTask PlayAsync(CancellationToken ct)
        {
            await _canvasGroup
                .DOFade(_param.Alpha, _param.DurationSec)
                .SetEase(_param.Ease)
                .ToUniTask(cancellationToken: ct);
        }
    }
}