// モジュール概要:
// QR コード表示ダイアログの View を実装し、Presenter から渡された Sprite を Image へ適用する役割を担う。
// 依存関係: UnityEngine.UI.Image を使用し、Common.UI.UIBehaviourBase を継承して共通 UI 機能を利用する。
// 使用例: QRCodeDialogPresenter が SetQRCodeSprite を呼び出し、生成した Sprite を UI へ反映する。

using Common.UI;
using UnityEngine;
using UnityEngine.UI;

namespace InGame.UI.Display.Dialog.QRCode
{
    /// <summary>
    /// 【目的】QR コードを表示するダイアログの View として、Image への Sprite 適用を担当する。
    /// 【理由】Presenter が生成した Sprite を即座に表示し、UI 側で追加処理を持たないシンプルな責務へ限定するため。
    /// </summary>
    public class QRCodeDialogView : UIBehaviourBase
    {
        /// <summary>
        /// 【目的】表示対象となる Image コンポーネントを Inspector から設定する。
        /// 【理由】ダイアログのプレハブごとに差し替えられるようにし、コード側での参照取得を不要にするため。
        /// </summary>
        [SerializeField, Header("QRコード画像")]
        private Image _qrCodeImage;
        
        /// <summary>
        /// 【目的】引数で受け取った QR コード Sprite を Image に適用する。
        /// 【理由】Presenter から生成済み Sprite を渡してもらい、View 側では単純な差し替えのみで完結させるため。
        /// </summary>
        /// <param name="sprite">【用途】表示する QR コード画像。null の場合は現状を維持する前提。</param>
        public void SetQRCodeSprite(Sprite sprite)
        {
            _qrCodeImage.sprite = sprite;
        }
    }
}
