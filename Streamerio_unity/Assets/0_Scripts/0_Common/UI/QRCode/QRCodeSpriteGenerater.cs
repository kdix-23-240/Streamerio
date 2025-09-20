using QRCoder;
using UnityEngine;

namespace InGame.UI.QRCode
{
    /// <summary>
    /// QRコードのSprite生成
    /// </summary>
    public class QRCodeSpriteGenerater
    {
        /// <summary>
        /// QRコードの画像生成
        /// </summary>
        /// <param name="qrText"></param>
        /// <returns></returns>
        public Sprite Generate(string qrText)
        {
            var qrGenerator = new QRCodeGenerator();
            var qrCodeData = qrGenerator.CreateQrCode(qrText, QRCodeGenerator.ECCLevel.Q);
            var qrCode = new PngByteQRCode(qrCodeData);
            byte[] pngData = qrCode.GetGraphic(20);

            // PNGバイト配列 → Texture2D変換
            Texture2D texture = new Texture2D(2, 2);
            texture.LoadImage(pngData);

            // Texture2D → Sprite変換
            return Sprite.Create(
                texture,
                new Rect(0, 0, texture.width, texture.height),
                new Vector2(0.5f, 0.5f)
            );
        }
    }
}