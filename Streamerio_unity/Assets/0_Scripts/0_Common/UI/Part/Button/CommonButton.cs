using Alchemy.Inspector;
using Common.UI.Animation;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Common.UI.Part.Button
{
    /// <summary>
    /// 共通のボタン。
    /// - 押下/解放/ホバー/離脱 に応じてアニメーションを再生
    /// - Scale / Fade を組み合わせた基本的な視覚効果を提供
    /// </summary>
    public class CommonButton : ButtonBase
    {
        [SerializeField, LabelText("ボタンを押した時のアニメーション")]
        private ScaleAnimationComponentParam _pushDownAnimParam = new()
        {
            Scale = 0.75f, 
            Duration = 0.1f,
            Ease = Ease.InSine,
        };

        [SerializeField, LabelText("ボタンを離した時のアニメーション")]
        private ScaleAnimationComponentParam _pushUpAnimParam = new()
        {
            Scale = 1f, 
            Duration = 0.1f,
            Ease = Ease.OutSine,
        };

        [SerializeField, LabelText("ボタンにカーソルがあった時のアニメーション")]
        private FadeAnimationComponentParam _enterAnimParam = new ()
        {
            Alpha = 0.5f,
            Duration = 0.1f,
            Ease = Ease.InSine,
        };

        [SerializeField, LabelText("ボタンにカーソルが離れた時のアニメーション")]
        private FadeAnimationComponentParam _exitAnimParam = new()
        {
            Alpha = 1f,
            Duration = 0.1f,
            Ease = Ease.OutSine,
        };

        private ScaleAnimationComponent _pushDownAnim;
        private ScaleAnimationComponent _pushUpAnim;
        private FadeAnimationComponent _enterAnim;
        private FadeAnimationComponent _exitAnim;
        
        /// <summary>
        /// 初期化処理。
        /// - 各種アニメーションコンポーネントを生成
        /// </summary>
        public override void Initialize()
        {
            base.Initialize();

            _pushDownAnim = new ScaleAnimationComponent(RectTransform, _pushDownAnimParam);
            _pushUpAnim = new ScaleAnimationComponent(RectTransform, _pushUpAnimParam);
            _enterAnim = new FadeAnimationComponent(CanvasGroup, _enterAnimParam);
            _exitAnim = new FadeAnimationComponent(CanvasGroup, _exitAnimParam);
        }

        /// <summary>
        /// 押下時の処理。
        /// - 縮小アニメーションを再生
        /// </summary>
        public override void OnPointerDown(PointerEventData eventData)
        {
            _pushDownAnim.PlayAsync(destroyCancellationToken).Forget();            
        }

        /// <summary>
        /// 解放時の処理。
        /// - 元のスケールに戻すアニメーションを再生
        /// </summary>
        public override void OnPointerUp(PointerEventData eventData)
        {
            _pushUpAnim.PlayAsync(destroyCancellationToken).Forget();
        }

        /// <summary>
        /// ホバー時の処理。
        /// - 半透明にするフェードアニメーションを再生
        /// </summary>
        public override void OnPointerEnter(PointerEventData eventData)
        {
            _enterAnim.PlayAsync(destroyCancellationToken).Forget();
        }

        /// <summary>
        /// ホバー解除時の処理。
        /// - フェードで元の透明度に戻す
        /// - スケールも押下前の状態にリセット
        /// </summary>
        public override void OnPointerExit(PointerEventData eventData)
        {
            _exitAnim.PlayAsync(destroyCancellationToken).Forget();
            _pushUpAnim.PlayAsync(destroyCancellationToken).Forget();
        }
        
        /// <summary>
        /// ボタンをデフォルト状態にリセット。
        /// - フェードを通常の透明度に戻す
        /// - スケールを等倍に戻す
        /// </summary>
        protected override void ResetButtonState()
        {
            CanvasGroup.alpha = _exitAnimParam.Alpha;
            RectTransform.localScale = _pushUpAnimParam.Scale * Vector3.one;
        }
    }
}