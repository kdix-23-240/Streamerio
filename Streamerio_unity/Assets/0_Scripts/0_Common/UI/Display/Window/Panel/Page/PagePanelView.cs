using System.Threading;
using Alchemy.Inspector;
using Common.UI.Part.Group;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Common.UI.Display.Window.Panel
{
    /// <summary>
    /// ページの見た目（View）を管理するクラス。
    /// - 内部の UI パーツグループ（CommonUIPartGroup）を使って表示/非表示を制御
    /// - CanvasGroup の透明度を直接制御して、全体の表示状態を管理
    /// - アニメーションと即時表示/非表示の両方に対応
    /// </summary>
    [RequireComponent(typeof(CommonUIPartGroup))]
    public class PagePanelView : DisplayViewBase
    {
        [SerializeField, ReadOnly]
        private CommonUIPartGroup _partGroup;

        [SerializeField, LabelText("表示の透明度")]
        private float _showAlpha = 1f;

        [SerializeField, LabelText("非表示の透明度")]
        private float _hideAlpha = 0f;

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();
            _partGroup ??= GetComponent<CommonUIPartGroup>();
        }
#endif

        /// <summary>
        /// 初期化処理。
        /// - 内部のパーツグループを初期化する
        /// </summary>
        public override void Initialize()
        {
            base.Initialize();
            _partGroup.Initialize();
        }

        /// <summary>
        /// アニメーションで表示。
        /// - 全体の CanvasGroup の透明度を設定
        /// - 内部のパーツを順次表示アニメーション
        /// </summary>
        public override async UniTask ShowAsync(CancellationToken ct)
        {
            CanvasGroup.alpha = _showAlpha;
            await _partGroup.ShowAsync(ct);
        }

        /// <summary>
        /// 即時表示。
        /// - CanvasGroup を即時透明度変更
        /// - 内部のパーツを即時表示
        /// </summary>
        public override void Show()
        {
            CanvasGroup.alpha = _showAlpha;
            _partGroup.Show();
        }

        /// <summary>
        /// アニメーションで非表示。
        /// - 内部のパーツを順次非表示アニメーション
        /// - 完了後に CanvasGroup を非表示状態にする
        /// </summary>
        public override async UniTask HideAsync(CancellationToken ct)
        {
            await _partGroup.HideAsync(ct);
            CanvasGroup.alpha = _hideAlpha;
        }

        /// <summary>
        /// 即時非表示。
        /// - 内部のパーツを即時非表示
        /// - CanvasGroup を非表示状態にする
        /// </summary>
        public override void Hide()
        {
            _partGroup.Hide();
            CanvasGroup.alpha = _hideAlpha;
        }
    }
}