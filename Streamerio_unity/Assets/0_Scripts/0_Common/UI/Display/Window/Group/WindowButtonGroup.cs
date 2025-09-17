using System.Threading;
using Alchemy.Inspector;
using Common.UI.Animation;
using Common.UI.Part.Button;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

namespace Common.UI.Display.Window.Group
{
    public class WindowButtonGroup: UIBehaviourBase
    {
        [SerializeField, LabelText("閉じるボタン")]
        private CommonButton _closeButton;
        public CommonButton CloseButton => _closeButton;
        [SerializeField, LabelText("次のページボタン")]
        private CommonButton _nextButton;
        public CommonButton NextButton => _nextButton;
        [SerializeField, LabelText("前のページボタン")]
        private CommonButton _backButton;
        public CommonButton BackButton => _backButton;
        
        [Header("アニメーション")]
        [SerializeField, LabelText("表示アニメーション")]
        private FadeAnimationComponentParam _showAnimationParam = new ()
        {
            Alpha = 1f,
            Duration = 0.1f,
            Ease = Ease.InSine,
        };
        [SerializeField, LabelText("非表示アニメーション")]
        private FadeAnimationComponentParam _hideAnimationParam = new ()
        {
            Alpha = 0f,
            Duration = 0.1f,
            Ease = Ease.OutSine,
        };
        
        private FadeAnimationComponent _showAnimation;
        private FadeAnimationComponent _hideAnimation;
        
        public override void Initialize()
        {
            base.Initialize();
            
            _closeButton.Initialize();
            _nextButton.Initialize();
            _backButton.Initialize();
            
            _showAnimation = new FadeAnimationComponent(CanvasGroup, _showAnimationParam);
            _hideAnimation = new FadeAnimationComponent(CanvasGroup, _hideAnimationParam);
        }
        
        /// <summary>
        /// 表示アニメーション再生
        /// </summary>
        /// <param name="isFirst"></param>
        /// <param name="isLast"></param>
        /// <param name="ct"></param>
        public async UniTask ShowAsync(bool isFirst, bool isLast, CancellationToken ct)
        {
            SetButtonsActive(isFirst, isLast);
            await _showAnimation.PlayAsync(ct);
        }
        
        /// <summary>
        /// 即時表示
        /// </summary>
        /// <param name="isFirst"></param>
        /// <param name="isLast"></param>
        public void Show(bool isFirst, bool isLast)
        {
            SetButtonsActive(isFirst, isLast);
            CanvasGroup.alpha = _showAnimationParam.Alpha;
        }
        
        /// <summary>
        /// 非表示アニメーション再生
        /// </summary>
        /// <param name="ct"></param>
        public async UniTask HideAsync(CancellationToken ct)
        {
            await _hideAnimation.PlayAsync(ct);
        }
        
        /// <summary>
        /// 非表示即時
        /// </summary>
        public void Hide()
        {
            CanvasGroup.alpha = _hideAnimationParam.Alpha;
        }
        
        /// <summary>
        /// ボタンのアクティブ設定
        /// </summary>
        /// <param name="isFirst"></param>
        /// <param name="isLast"></param>
        private void SetButtonsActive(bool isFirst, bool isLast)
        {
            _backButton.gameObject.SetActive(!isFirst);
            _nextButton.gameObject.SetActive(!isLast);
        }
    }
}