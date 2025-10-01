using System.Threading;
using Alchemy.Inspector;
using Common.UI.Animation;
using Common.UI.Display.Background;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

namespace Common.UI.Display.Window
{
    /// <summary>
    /// 共通ウィンドウの見た目を管理するクラス。
    /// - 背景 (DisplayBackgroundPresenter) と本体 (RectTransform) をまとめて制御
    /// - DOTween を使って表示/非表示の移動アニメーションを再生
    /// - 即時表示/非表示もサポート
    /// </summary>
    public class CommonWindowView : DisplayViewBase
    {
        [SerializeField, LabelText("本体のRectTransform")]
        private RectTransform _bookRectTransform; // ウィンドウ本体の RectTransform

        [SerializeField, ReadOnly]
        private DisplayBackgroundPresenter _background;
        /// <summary> 背景の表示/非表示を管理する Presenter </summary>
        public DisplayBackgroundPresenter Background => _background;
        
        [SerializeField, LabelText("初期位置")]
        private Vector2 _initialPosition; // 表示前の初期位置（アニメ開始位置）

        [Header("アニメーション設定")]
        [SerializeField, LabelText("表示アニメーション")]
        private MoveAnimationComponentParam _showAnimParam = new()
        {
            AnchoredPosition = Vector2.zero,
            DurationSec = 0.2f,
            Ease = Ease.InSine,
        };

        [SerializeField, LabelText("非表示アニメーション")]
        private MoveAnimationComponentParam _hideAnimParam = new()
        {
            AnchoredPosition = Vector2.zero,
            DurationSec = 0.2f,
            Ease = Ease.OutSine,
        };

        private MoveAnimationComponent _showAnim; // 表示アニメーション
        private MoveAnimationComponent _hideAnim; // 非表示アニメーション

#if UNITY_EDITOR
        /// <summary>
        /// Inspector 上で参照が未設定なら自動補完。
        /// </summary>
        protected override void OnValidate()
        {
            base.OnValidate();
            _background ??= GetComponentInChildren<DisplayBackgroundPresenter>();
        }
#endif

        /// <summary>
        /// 初期化処理。
        /// - 背景の初期化＆非表示化
        /// - MoveAnimationComponent を生成して準備
        /// - ウィンドウを初期位置にセット
        /// </summary>
        public override void Initialize()
        {
            base.Initialize();

            _background.Initialize();
            _background.Hide();

            _showAnim = new MoveAnimationComponent(_bookRectTransform, _showAnimParam);
            _hideAnim = new MoveAnimationComponent(_bookRectTransform, _hideAnimParam);
            
            // 初期位置を強制設定
            _bookRectTransform.anchoredPosition = _initialPosition;
        }

        /// <summary>
        /// 表示処理（アニメーションあり）。
        /// - 初期位置に戻してから背景を表示
        /// - 移動アニメーションでウィンドウを表示
        /// </summary>
        public override async UniTask ShowAsync(CancellationToken ct)
        {
            _bookRectTransform.anchoredPosition = _initialPosition;
            await _background.ShowAsync(ct);
            await _showAnim.PlayAsync(ct);
        }

        /// <summary>
        /// 即時表示（アニメーションなし）。
        /// - 背景を即時表示
        /// - ウィンドウ本体を最終位置に強制配置
        /// </summary>
        public override void Show()
        {
            _background.Show();
            _bookRectTransform.anchoredPosition = _showAnimParam.AnchoredPosition;
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
        }

        /// <summary>
        /// 即時非表示（アニメーションなし）。
        /// - ウィンドウ本体を非表示位置へ強制移動
        /// - 背景を即時非表示
        /// </summary>
        public override void Hide()
        {
            _bookRectTransform.anchoredPosition = _hideAnimParam.AnchoredPosition;
            _background.Hide();
        }
    }
}
