using System.Threading;
using Alchemy.Inspector;
using Common.UI.Animation;
using Common.UI.Display;
using Common.UI.Part.Text;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

namespace OutGame.UI.Display.Screen
{
    /// <summary>
    /// タイトル画面の View。
    /// - 画面全体のフェードイン/アウトを制御
    /// - 「PRESS START」等の点滅テキスト(FlashText)を開始/停止
    /// </summary>
    public class TitleScreenView : DisplayViewBase
    {
        [SerializeField]
        private FlashText _gameStartText;

        [Header("アニメーション")]
        [SerializeField, LabelText("表示アニメーション")]
        private FadeAnimationComponentParam _showFadeAnimationParam = new ()
        {
            Alpha = 1f,
            DurationSec = 0.1f,
            Ease = Ease.InSine,
        };

        [SerializeField, LabelText("非表示アニメーション")]
        private FadeAnimationComponentParam _hideFadeAnimationParam = new ()
        {
            Alpha = 0f,
            DurationSec = 0.1f,
            Ease = Ease.OutSine,
        };
        
        private FadeAnimationComponent _showAnimation;
        private FadeAnimationComponent _hideAnimation;

        /// <summary>
        /// 初期化。
        /// - 点滅テキストの初期化
        /// - フェード用コンポーネントの構築
        /// </summary>
        public override void Initialize()
        {
            base.Initialize();
            
            _gameStartText.Initialize();
            _showAnimation = new FadeAnimationComponent(CanvasGroup, _showFadeAnimationParam);
            _hideAnimation = new FadeAnimationComponent(CanvasGroup, _hideFadeAnimationParam);
        }
        
        /// <summary>
        /// アニメーションで表示。
        /// - フェードイン完了後、点滅テキストを開始
        /// </summary>
        public override async UniTask ShowAsync(CancellationToken ct)
        {
            await _showAnimation.PlayAsync(ct);
            _gameStartText.PlayTextAnimation();
        }
        
        /// <summary>
        /// 即時表示。
        /// - CanvasGroup のアルファを表示側パラメータに設定
        /// - 点滅テキストを開始
        /// </summary>
        public override void Show()
        {
            CanvasGroup.alpha = _showFadeAnimationParam.Alpha;
            _gameStartText.PlayTextAnimation();
        }
        
        /// <summary>
        /// アニメーションで非表示。
        /// - フェードアウト完了後、点滅テキストを停止
        /// </summary>
        public override async UniTask HideAsync(CancellationToken ct)
        {
            await _hideAnimation.PlayAsync(ct);
            _gameStartText.StopTextAnimation();
        }
        
        /// <summary>
        /// 即時非表示。
        /// - CanvasGroup のアルファを非表示側パラメータに設定
        /// - 点滅テキストを停止
        /// </summary>
        public override void Hide()
        {
            CanvasGroup.alpha = _hideFadeAnimationParam.Alpha;
            _gameStartText.StopTextAnimation();
        }
    }
}
