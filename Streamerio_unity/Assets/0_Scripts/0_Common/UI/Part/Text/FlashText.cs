using System.Threading;
using Alchemy.Inspector;
using Common.UI.Animation;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Common.UI.Part.Text
{
    /// <summary>
    /// 点滅するテキスト
    /// </summary>
    public class FlashText: UIBehaviourBase
    {
        [SerializeField, LabelText("テキストの点滅アニメーション")]
        private FlashAnimationComponentParam _flashAnimationParam;
        
        private FlashAnimationComponent _flashAnimation;

        private CancellationTokenSource _cts;

        public override void Initialize()
        {
            base.Initialize();
            
            _flashAnimation = new FlashAnimationComponent(CanvasGroup, _flashAnimationParam);
        }
        
        /// <summary>
        /// テキストのアニメーション再生
        /// </summary>
        public void PlayTextAnimation()
        {
            _cts = new CancellationTokenSource();
            _flashAnimation.PlayAsync(_cts.Token).Forget();
        }
        
        /// <summary>
        /// テキストのアニメーションを止める
        /// </summary>
        public void StopTextAnimation()
        {
            _cts?.Cancel();
            _cts?.Dispose();
            
            CanvasGroup.alpha = _flashAnimationParam.MinAlpha;
        }
    }
}