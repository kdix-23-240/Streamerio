using System.Threading;
using Alchemy.Inspector;
using Common.UI.Animation;
using Common.UI.Display;
using Common.UI.Display.Background;
using Common.UI.Part.Button;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Common.UI.Dialog
{
    /// <summary>
    /// 共通ダイアログの View。
    /// - 背景と閉じるボタンを保持
    /// - RectTransform の移動アニメーションで表示/非表示を制御
    /// </summary>
    public class CommonDialogView : DisplayViewBase
    {
        [SerializeField, ReadOnly]
        private DisplayBackgroundPresenter _background;
        /// <summary>ダイアログ背面に敷かれる背景</summary>
        public DisplayBackgroundPresenter Background => _background;

        [SerializeField, LabelText("動かすオブジェクト")]
        private RectTransform _moveRectTransform;
        
        [SerializeField, LabelText("閉じるボタン")]
        private CommonButton _closeButton;
        /// <summary>閉じる用のボタン</summary>
        public CommonButton CloseButton => _closeButton;

        [Header("アニメーション")]
        [SerializeField, LabelText("表示アニメーション")]
        private PathAnimationComponentParam _showAnimationParam;
        [SerializeField, LabelText("非表示アニメーション")]
        private PathAnimationComponentParam _hideAnimationParam;

        private PathAnimationComponent _showAnimation;
        private PathAnimationComponent _hideAnimation;

#if UNITY_EDITOR
        /// <summary>
        /// エディタ上で参照を自動補完
        /// </summary>
        protected override void OnValidate()
        {
            base.OnValidate();
            _background ??= GetComponentInChildren<DisplayBackgroundPresenter>();
        }
#endif
        
        /// <summary>
        /// 初期化処理。
        /// - 背景と閉じるボタンを初期化
        /// - 表示/非表示用のアニメーションコンポーネントを生成
        /// </summary>
        public override void Initialize()
        {
            base.Initialize();
            
            _background.Initialize();
            _closeButton.Initialize();

            _showAnimation = new PathAnimationComponent(_moveRectTransform, _showAnimationParam);
            _hideAnimation = new PathAnimationComponent(_moveRectTransform, _hideAnimationParam);
        }
        
        /// <summary>
        /// アニメーションで表示。
        /// - 移動開始位置に配置
        /// - 背景をフェードイン
        /// - 本体をアニメーションで移動表示
        /// </summary>
        public override async UniTask ShowAsync(CancellationToken ct)
        {
            _moveRectTransform.anchoredPosition = _showAnimationParam.Path[0];
            await _background.ShowAsync(ct);
            await _showAnimation.PlayAsync(ct);
        }
        
        /// <summary>
        /// 即時表示。
        /// - 移動完了位置に配置
        /// - 背景を即時表示
        /// </summary>
        public override void Show()
        {
            _moveRectTransform.anchoredPosition = _showAnimationParam.Path[^1];
            _background.Show();
        }
        
        /// <summary>
        /// アニメーションで非表示。
        /// - 移動開始位置に配置
        /// - 背景をフェードアウト
        /// - 本体をアニメーションで移動非表示
        /// </summary>
        public override async UniTask HideAsync(CancellationToken ct)
        {
            _moveRectTransform.anchoredPosition = _hideAnimationParam.Path[0];
            await _background.HideAsync(ct);
            await _hideAnimation.PlayAsync(ct);
        }
        
        /// <summary>
        /// 即時非表示。
        /// - 背景を即時非表示
        /// - 移動完了位置に配置
        /// </summary>
        public override void Hide()
        {
            _background.Hide();
            _moveRectTransform.anchoredPosition = _hideAnimationParam.Path[^1];
        }
    }
}
