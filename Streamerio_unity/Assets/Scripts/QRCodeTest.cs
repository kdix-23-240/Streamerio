// Assets/Scripts/QRCodeTest.cs

using UnityEngine;
using QRCoder;

public class QRCodeTest : MonoBehaviour
{
    [Header("表示するQRコードのテキスト")]
    public string qrText = "UnityでQRコード！";

    [Header("QRコードを表示するRenderer")]
    public Renderer targetRenderer;

    void Start()
    {
        // QRコード生成
        var qrGenerator = new QRCodeGenerator();
        var qrCodeData = qrGenerator.CreateQrCode(qrText, QRCodeGenerator.ECCLevel.Q);
        var qrCode = new PngByteQRCode(qrCodeData);
        byte[] qrCodeImage = qrCode.GetGraphic(20);

        // PNGバイト配列 → Texture2D変換
        Texture2D texture = new Texture2D(2, 2);
        texture.LoadImage(qrCodeImage);

        // GameObjectに表示
        if (targetRenderer != null)
        {
            targetRenderer.material.mainTexture = texture;
        }

        Debug.Log("QRコードを生成して表示しました: " + qrText);
    }
}

