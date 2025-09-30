using System.Threading;
using Alchemy.Inspector;
using Common.UI.Animation;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Common.UI.Part.Text
{
    /// <summary>
    /// テキストを点滅させるコンポーネント。
    /// - FlashAnimationComponent を利用してアルファ値を変化させる
    /// - Play/Stop メソッドでアニメーションを制御
    /// </summary>
    public class FlashText : UIBehaviourBase
    {
        [SerializeField, LabelText("テキストの点滅アニメーション設定")]
        private FlashAnimationComponentParam _flashAnimationParam;
        
        private FlashAnimationComponent _flashAnimation;
        private CancellationTokenSource _cts;

        /// <summary>
        /// 初期化処理。
        /// - 点滅アニメーションのインスタンスを生成
        /// </summary>
        public override void Initialize()
        {
            base.Initialize();
            _flashAnimation = new FlashAnimationComponent(CanvasGroup, _flashAnimationParam);
        }
        
        /// <summary>
        /// テキストの点滅アニメーションを開始。
        /// - 新しい CancellationTokenSource を生成
        /// - 非同期でアルファ値を変化させる
        /// </summary>
        public void PlayTextAnimation()
        {
            _cts = new CancellationTokenSource();
            _flashAnimation.PlayAsync(_cts.Token).Forget();
        }
        
        /// <summary>
        /// テキストの点滅アニメーションを停止。
        /// - トークンをキャンセルし破棄
        /// - アルファ値を最小値にリセット
        /// </summary>
        public void StopTextAnimation()
        {
            _cts?.Cancel();
            _cts?.Dispose();
            
            CanvasGroup.alpha = _flashAnimationParam.MinAlpha;
        }
    }
}