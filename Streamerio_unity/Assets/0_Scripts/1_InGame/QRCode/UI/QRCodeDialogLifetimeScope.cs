// モジュール概要:
// QR コード表示ダイアログ用の VContainer LifetimeScope を構築し、View・Presenter・サービスを登録して Wiring する。
// 依存関係: DialogLifetimeScopeBase の共通登録に加え、QRCodeDialogView と IQRCodeService を解決する。
// 使用例: QR コードダイアログのプレハブにアタッチし、表示時に QR コード生成サービスが注入されるようにする。

using Common.QRCode;
using Common.UI.Dialog;
using Common.UI.Display;
using VContainer;

namespace InGame.QRCode.UI
{
    /// <summary>
    /// 【目的】QR コードダイアログの依存登録をまとめ、Presenter が必要とする View/サービスを提供する。
    /// 【理由】ダイアログごとに重複する登録処理を避け、LifetimeScope で構成を一元化するため。
    /// </summary>
    public class QRCodeDialogLifetimeScope: DialogLifetimeScopeBase<IQRCodeDialog, QRCodeDialogPresenter, IQRCodeDialogView, QRCodeDialogContext>
    {
        protected override QRCodeDialogContext CreateContext(IObjectResolver resolver)
        {
            return new QRCodeDialogContext
            {
                View = resolver.Resolve<IQRCodeDialogView>(),
                Service = resolver.Resolve<IDialogService>(),
                QRCodeService = resolver.Resolve<IQRCodeService>()
            };
        }
    }
}
