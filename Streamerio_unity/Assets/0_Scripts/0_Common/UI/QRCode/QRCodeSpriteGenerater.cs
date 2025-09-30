using Cysharp.Threading.Tasks;
using QRCoder;
using UnityEngine;

namespace InGame.UI.QRCode
{
    /// <summary>
    /// QRコードのSprite生成
    /// </summary>
    public static class QRCodeSpriteGenerater
    {
        private static Sprite _qrCodeSprite;
        
        /// <summary>
        /// 初期化
        /// </summary>
        /// <param name="qrText"></param>
        public static async UniTask InitializeSprite(string qrText)
        {
            _qrCodeSprite = Generate(qrText);
        }
        
        /// <summary>
        /// QRコードの画像生成
        /// </summary>
        /// <param name="qrText"></param>
        /// <returns></returns>
        private static Sprite Generate(string qrText)
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

        public static Sprite GetQRCodeSprite()
        {
            return _qrCodeSprite;
        }
    }
}