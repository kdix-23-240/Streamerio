// モジュール概要:
// QR コード表示ダイアログの Presenter を実装し、生成サービスからの Sprite 更新を受けて View へ反映する。
// 依存関係: Common.QRCode.IQRCodeService で Sprite を生成し、Common.UI.Dialog.DialogPresenterBase の機能で共通ダイアログ挙動を利用する。
// 使用例: QRCodeDialogLifetimeScope が本 Presenter を登録し、表示時に UpdateSprite を呼び出して UI に QR コードを表示する。

using Common.QRCode;
using Common.UI.Dialog;
using R3;

namespace InGame.UI.Display.Dialog.QRCode
{
    /// <summary>
    /// 【目的】QR コード表示ダイアログの View とサービスを接続し、生成された Sprite をリアクティブに適用する。
    /// 【理由】生成ロジックをサービスへ委譲しつつ、UI 更新のみを Presenter で担うことで責務を明確に分離するため。
    /// </summary>
    public class QRCodeDialogPresenter:DialogPresenterBase<QRCodeDialogContext>
    {
        /// <summary>
        /// 【目的】Sprite を適用する View を保持する。
        /// 【理由】AttachContext 以降で何度もアクセスするため、フィールドにキャッシュして高速化する。
        /// </summary>
        private QRCodeDialogView _view;
        /// <summary>
        /// 【目的】QR コード画像を生成・管理するサービスを保持する。
        /// 【理由】SpriteProp を購読して View を更新し、サービス側の生成ロジック再利用を可能にするため。
        /// </summary>
        private IQRCodeService _qrCodeService;

        /// <summary>
        /// 【目的】文脈から View と QR コードサービスを取得し、フィールドへ割り当てる。
        /// 【理由】基底クラスの Attach 処理に加えて、QR コード固有依存を Presenter 内で利用可能にするため。
        /// </summary>
        /// <param name="context">【用途】View、サービス、共通ダイアログ要素をまとめたコンテキスト。</param>
        protected override void AttachContext(QRCodeDialogContext context)
        {
            base.AttachContext(context);
            
            _view = context.View;
            _qrCodeService = context.QRCodeService;
        }

        /// <summary>
        /// 【目的】QR コード Sprite の更新を購読し、ビューへ反映する。
        /// 【理由】サービス側で URL 更新が行われた際に UI が即座に切り替わるようにするため。
        /// </summary>
        protected override void Bind()
        {
            base.Bind();

            _qrCodeService.SpriteProp
                .DistinctUntilChanged()
                .Where(sprite => sprite != null)
                .Subscribe(sprite =>
                {
                    _view.SetQRCodeSprite(sprite);
                })
                .RegisterTo(GetCt());
        }
    }
    
    /// <summary>
    /// 【目的】QRCodeDialogPresenter が利用する依存をまとめたコンテキスト。
    /// 【理由】LifetimeScope から Presenter へ一括で渡し、AttachContext 内で抽出できるようにするため。
    /// </summary>
    public class QRCodeDialogContext: CommonDialogContext
    {
        /// <summary>
        /// 【目的】QR コード画像を表示する View を保持する。
        /// 【理由】Presenter が Sprite 適用先を参照する際に必要になるため。
        /// </summary>
        public QRCodeDialogView View;
        /// <summary>
        /// 【目的】QR コード Sprite を生成・保持するサービスを提供する。
        /// 【理由】Presenter が SpriteProp を購読し、UI 更新を行うため。
        /// </summary>
        public IQRCodeService QRCodeService; 
    }
}
