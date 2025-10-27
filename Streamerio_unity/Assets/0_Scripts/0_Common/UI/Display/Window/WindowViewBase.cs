using System.Threading;
using Alchemy.Inspector;
using Common.UI.Animation;
using Common.UI.Display.Background;
using Common.UI.Part.Button;
using Cysharp.Threading.Tasks;
using UnityEngine;
using VContainer;

namespace Common.UI.Display.Window
{
    /// <summary>
    /// 共通ウィンドウの見た目を管理するクラス。
    /// - 背景 (DisplayBackgroundPresenter) と本体 (RectTransform) をまとめて制御
    /// - DOTween を使って表示/非表示の移動アニメーションを再生
    /// - 即時表示/非表示もサポート
    /// </summary>
    public abstract class WindowViewBase : DisplayViewBase, IWindowView
    {
        [SerializeField, LabelText("本体のRectTransform")]
        private RectTransform _displayRectTransform; // ウィンドウ本体の RectTransform

        private IDisplayBackground _background;
        
        private ICommonButton _closeButton;
        public ICommonButton CloseButton => _closeButton;

        private IUIAnimation _showAnim; // 表示アニメーション
        private IUIAnimation _hideAnim; // 非表示アニメーション

        private IUIAnimation _showPartsAnim;
        private IUIAnimation _hidePartsAnim;
        
        [Inject]
        public void Construct(IDisplayBackground background, 
            [Key(ButtonType.Close)] ICommonButton closeButton,
            [Key(AnimationType.Show)] IUIAnimation showAnim,
            [Key(AnimationType.Hide)] IUIAnimation hideAnim,
            [Key(AnimationType.ShowParts)] IUIAnimation showPartsAnim,
            [Key(AnimationType.HideParts)] IUIAnimation hidePartsAnim)
        {
            _background = background;
            _closeButton = closeButton;
            
            _showAnim = showAnim;
            _hideAnim = hideAnim;
            
            _showPartsAnim = showPartsAnim;
            _hidePartsAnim = hidePartsAnim;
        }
        
        /// <summary>
        /// 表示処理（アニメーションあり）。
        /// - 初期位置に戻してから背景を表示
        /// - 移動アニメーションでウィンドウを表示
        /// </summary>
        public override async UniTask ShowAsync(CancellationToken ct)
        {
            await _background.ShowAsync(ct);
            await _showAnim.PlayAsync(ct);
            await _showPartsAnim.PlayAsync(ct);
        }

        /// <summary>
        /// 即時表示（アニメーションなし）。
        /// - 背景を即時表示
        /// - ウィンドウ本体を最終位置に強制配置
        /// </summary>
        public override void Show()
        {
            _background.Show();
            _showAnim.PlayImmediate();
            _showPartsAnim.PlayImmediate();
        }

        /// <summary>
        /// 非表示処理（アニメーションあり）。
        /// - 移動アニメーションでウィンドウを隠す
        /// - 背景を非表示
        /// </summary>
        public override async UniTask HideAsync(CancellationToken ct)
        {
            await _hideAnim.PlayAsync(ct);
            await _background.HideAsync(ct);
            _hidePartsAnim.PlayImmediate();
        }

        /// <summary>
        /// 即時非表示（アニメーションなし）。
        /// - ウィンドウ本体を非表示位置へ強制移動
        /// - 背景を即時非表示
        /// </summary>
        public override void Hide()
        {
            _hideAnim.PlayImmediate();
            _hidePartsAnim.PlayImmediate();
            _background.Hide();
        }
    }
    
    public interface IWindowView : IDisplayView
    {
        ICommonButton CloseButton { get; }
    }
}
