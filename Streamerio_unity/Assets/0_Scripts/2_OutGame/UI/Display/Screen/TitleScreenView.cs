using System.Threading;
using Common.UI;
using Common.UI.Animation;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace OutGame.UI.Display.Screen
{
    /// <summary>
    /// タイトル画面の見た目
    /// </summary>
    public class TitleScreenView: UIBehaviourBase
    {
        [SerializeField]
        private CanvasGroup _gameStartText;

        [Header("アニメーション")]
        [SerializeField]
        private FlashAnimationComponentParam _flashAnimationParam;

        private FlashAnimationComponent _flashAnimation;

        private CancellationTokenSource _cts;
        
        public override void Initialize()
        {
            base.Initialize();
            _flashAnimation = new FlashAnimationComponent(_gameStartText, _flashAnimationParam);
        }
        
        /// <summary>
        /// 表示
        /// </summary>
        public void Show()
        {
            CanvasGroup.alpha = 1f;
            _cts = new CancellationTokenSource();
            PlayStartTextAnimation();
        }
        
        /// <summary>
        /// 非表示
        /// </summary>
        public void Hide()
        {
            CanvasGroup.alpha = 0f;
            StopStartTextAnimation();
        }
        
        /// <summary>
        /// テキストのアニメーション再生
        /// </summary>
        private void PlayStartTextAnimation()
        {
            _flashAnimation.PlayAsync(_cts.Token).Forget();
        }
        
        /// <summary>
        /// テキストのアニメーションを止める
        /// </summary>
        private void StopStartTextAnimation()
        {
            _cts.Cancel();
            _cts.Dispose();
            
            _gameStartText.alpha = _flashAnimationParam.MaxAlpha;
        }
    }
}