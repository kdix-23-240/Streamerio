// モジュール概要:
// QR コード表示ダイアログ用の VContainer LifetimeScope を構築し、View・Presenter・サービスを登録して Wiring する。
// 依存関係: DialogLifetimeScopeBase の共通登録に加え、QRCodeDialogView と IQRCodeService を解決する。
// 使用例: QR コードダイアログのプレハブにアタッチし、表示時に QR コード生成サービスが注入されるようにする。

using Common;
using Common.QRCode;
using Common.UI.Dialog;
using VContainer;
using VContainer.Unity;

namespace InGame.UI.Display.Dialog.QRCode
{
    /// <summary>
    /// 【目的】QR コードダイアログの依存登録をまとめ、Presenter が必要とする View/サービスを提供する。
    /// 【理由】ダイアログごとに重複する登録処理を避け、LifetimeScope で構成を一元化するため。
    /// </summary>
    public class QRCodeDialogLifetimeScope: DialogLifetimeScopeBase
    {
        /// <summary>
        /// 【目的】QR コードダイアログ固有の依存を登録し、Wiring を構成する。
        /// 【処理概要】共通登録→View 登録→Presenter 登録→Wiring にコンテキストを渡す。
        /// 【理由】Presenter が View とサービスへ確実にアクセスできる状態を作り、Attach/Detach の呼び出しを自動化するため。
        /// </summary>
        /// <param name="builder">【用途】依存登録とエントリポイント設定を行う VContainer のビルダー。</param>
        protected override void Configure(IContainerBuilder builder)
        {
            base.Configure(builder);
            
            var view = GetComponent<QRCodeDialogView>();
            builder.RegisterComponent(view);
            
            builder.Register<QRCodeDialogPresenter>(Lifetime.Singleton)
                .AsSelf()
                .As<IStartable>();
            builder.RegisterEntryPoint<Wiring<QRCodeDialogPresenter, QRCodeDialogContext>>()
                .WithParameter(resolver => resolver.Resolve<QRCodeDialogPresenter>())
                .WithParameter(resolver =>
                {
                    return new QRCodeDialogContext
                    {
                        CommonView = resolver.Resolve<ICommonDialogView>(),
                        Service = resolver.Resolve<IDialogService>(),
                        View = view,
                        QRCodeService = resolver.Resolve<IQRCodeService>()
                    };
                });
        }
    }
}
