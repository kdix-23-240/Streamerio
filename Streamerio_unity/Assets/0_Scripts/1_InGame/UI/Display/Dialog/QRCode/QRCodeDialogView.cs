using Common.UI.Dialog;
using UnityEngine;
using UnityEngine.UI;

namespace InGame.UI.Display.Dialog.QRCode
{
    /// <summary>
    /// QRコードを表示するダイアログの見た目
    /// </summary>
    public class QRCodeDialogView: DialogViewBase
    {
        [SerializeField, Header("QRコード画像")]
        private Image _qrCodeImage;
        
        /// <summary>
        /// QRコードのスプライトを設定
        /// </summary>
        /// <param name="sprite"></param>
        public void SetQRCodeSprite(Sprite sprite)
        {
            _qrCodeImage.sprite = sprite;
        }
    }
}