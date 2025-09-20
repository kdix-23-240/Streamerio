// Assets/Scripts/QRCodeUIImageTest.cs

using UnityEngine;
using UnityEngine.UI;
using QRCoder;

public class QRCodeUIImageTest : MonoBehaviour
{
    [Header("表示するQRコードのテキスト")]
    public string qrText = "Unity UI QRコード！";

    [Header("QRコードを表示するImage")]
    public Image targetImage;

    void Start()
    {
        // QRコード生成
        var qrGenerator = new QRCodeGenerator();
        var qrCodeData = qrGenerator.CreateQrCode(qrText, QRCodeGenerator.ECCLevel.Q);
        var qrCode = new PngByteQRCode(qrCodeData);
        byte[] pngData = qrCode.GetGraphic(20);

        // PNGバイト配列 → Texture2D変換
        Texture2D texture = new Texture2D(2, 2);
        texture.LoadImage(pngData);

        // Texture2D → Sprite変換
        Sprite qrSprite = Sprite.Create(
            texture,
            new Rect(0, 0, texture.width, texture.height),
            new Vector2(0.5f, 0.5f)
        );

        // Imageに表示
        if (targetImage != null)
        {
            targetImage.sprite = qrSprite;
        }

        Debug.Log("QRコードをUI Imageに表示しました: " + qrText);
    }
}

