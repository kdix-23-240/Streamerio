using Alchemy.Inspector;
using Common.UI.Dialog;
using InGame.UI.QRCode;
using UnityEngine;

namespace InGame.UI.Display.Dialog.QRCode
{
    /// <summary>
    /// QRコード表示用ダイアログの Presenter。
    /// - View の初期化
    /// - QRコード画像の生成・セット
    /// - 共通ダイアログのイベント購読処理は基底クラスに委譲
    /// </summary>
    [RequireComponent(typeof(QRCodeDialogView))]
    public class QRCodeDialogPresenter : DialogPresenterBase
    {
        [SerializeField, ReadOnly]
        private QRCodeDialogView _view;

#if UNITY_EDITOR
        /// <summary>
        /// エディタ上でコンポーネント参照を自動補完
        /// </summary>
        protected override void OnValidate()
        {
            base.OnValidate();
            _view ??= GetComponent<QRCodeDialogView>();
        }
#endif

        /// <summary>
        /// 初期化処理。
        /// - View の初期化
        /// - QRコード画像を生成してセット
        /// - 基底クラスの初期化でイベント購読を設定
        /// </summary>
        public override void Initialize()
        {
            _view.Initialize();
            _view.SetQRCodeSprite(QRCodeSpriteGenerater.GetQRCodeSprite());
            base.Initialize();
        }
    }
}