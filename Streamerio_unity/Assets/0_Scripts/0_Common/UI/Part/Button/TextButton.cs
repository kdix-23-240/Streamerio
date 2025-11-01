using Alchemy.Inspector;
using Common.UI.Animation;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Common.UI.Part.Button
{
    /// <summary>
    /// テキストボタン。
    /// - ホバー時に装飾（羽・下線）を表示
    /// - 押下/解放時にスケールアニメーションを再生
    /// </summary>
    public class TextButton : ButtonBase
    {
        [Header("装飾画像")]
        [SerializeField, LabelText("羽")]
        private Image _featherImage;

        [SerializeField, LabelText("下線")]
        private Image _lineImage;
        
        [SerializeField, LabelText("ボタンを押した時のアニメーション")]
        private ScaleAnimationComponentParam _pushDownAnimParam = new()
        {
            Scale = 0.75f, 
            DurationSec = 0.1f,
            Ease = Ease.InSine,
        };

        [SerializeField, LabelText("ボタンを離した時のアニメーション")]
        private ScaleAnimationComponentParam _pushUpAnimParam = new()
        {
            Scale = 1f, 
            DurationSec = 0.1f,
            Ease = Ease.OutSine,
        };
        
        private ScaleAnimationComponent _pushDownAnim;
        private ScaleAnimationComponent _pushUpAnim;

        /// <summary>
        /// 初期化処理。
        /// - 羽と下線を非表示に設定
        /// - スケールアニメーションコンポーネントを生成
        /// </summary>
        public override void Initialize()
        {
            base.Initialize();
            
            _featherImage.enabled = false;
            _lineImage.enabled = false;
            
            _pushDownAnim = new ScaleAnimationComponent(RectTransform, _pushDownAnimParam);
            _pushUpAnim = new ScaleAnimationComponent(RectTransform, _pushUpAnimParam);
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
        /// ホバー開始時の処理。
        /// - 羽と下線を表示
        /// </summary>
        public override void OnPointerEnter(PointerEventData eventData)
        {
            _featherImage.enabled = true;
            _lineImage.enabled = true;
        }
        
        /// <summary>
        /// ホバー終了時の処理。
        /// - 羽と下線を非表示
        /// - 元のスケールに戻すアニメーションを再生
        /// </summary>
        public override void OnPointerExit(PointerEventData eventData)
        {
            _pushUpAnim.PlayAsync(destroyCancellationToken).Forget();
            
            _featherImage.enabled = false;
            _lineImage.enabled = false;
        }
        
        /// <summary>
        /// ボタンをデフォルト状態にリセット。
        /// - スケールを初期値に戻す
        /// - 羽と下線を非表示にする
        /// </summary>
        protected override void ResetButtonState()
        {
            RectTransform.localScale = _pushDownAnimParam.Scale * Vector3.one;
            _featherImage.enabled = false;
            _lineImage.enabled = false;
        }
    }
}