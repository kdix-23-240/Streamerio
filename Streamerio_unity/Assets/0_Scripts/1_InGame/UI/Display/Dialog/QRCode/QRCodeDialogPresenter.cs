using System;
using Common.UI.Dialog;
using Common.UI.Display;
using UnityEngine;

namespace InGame.UI.Display.Dialog.QRCode
{
    /// <summary>
    /// QRコードを表示するダイアログのつなぎ役
    /// </summary>
    [RequireComponent(typeof(QRCodeDialogView))]
    [Serializable]
    public class QRCodeDialogPresenter: DialogPresenterBase<QRCodeDialogView>
    {
        /// <summary>
        /// QRコードの画像を設定
        /// </summary>
        /// <param name="sprite"></param>
        public void SetQRCodeSprite(Sprite sprite)
        {
            View.SetQRCodeSprite(sprite);
        }

        protected override void CloseEvent()
        {
            InGameManager.Instance.StartGame();
        }
    }
}