using Common.UI;
using UnityEngine;
using UnityEngine.UI;

namespace InGame.UI.Display.Dialog.QRCode
{
    /// <summary>
    /// QRコードを表示するダイアログの View。
    /// - Image コンポーネントに QRコード画像を表示
    /// - Presenter から渡されたスプライトを適用するだけのシンプルな役割
    /// </summary>
    public class QRCodeDialogView : UIBehaviourBase
    {
        [SerializeField, Header("QRコード画像")]
        private Image _qrCodeImage;
        
        /// <summary>
        /// QRコードのスプライトを Image に適用する。
        /// </summary>
        /// <param name="sprite">表示する QRコード画像</param>
        public void SetQRCodeSprite(Sprite sprite)
        {
            _qrCodeImage.sprite = sprite;
        }
    }
}