using Common.UI.Display;
using Cysharp.Threading.Tasks;
using R3;
using UnityEngine;

namespace Common.UI.Dialog
{
    /// <summary>
    /// 共通ダイアログの Presenter 基底クラス。
    /// - Close ボタン押下で閉じる
    /// - 背景クリックで閉じる
    /// </summary>
    [RequireComponent(typeof(CommonDialogView))]
    public abstract class DialogPresenterBase : DisplayPresenterBase<CommonDialogView>
    {
        /// <summary>
        /// イベント購読のセットアップ。
        /// - Close ボタン → CloseEvent()
        /// - 背景 → CloseEvent()
        /// </summary>
        protected override void Bind()
        {
            base.Bind();
            
            // Close ボタン押下でダイアログを閉じる
            CommonView.CloseButton.OnClickAsObservable
                .Subscribe(_ => CloseEvent())
                .RegisterTo(destroyCancellationToken);
            
            // 背景クリックでもダイアログを閉じる
            CommonView.Background.OnClickAsObservable
                .Subscribe(_ => CloseEvent())
                .RegisterTo(destroyCancellationToken);
        }

        /// <summary>
        /// ダイアログを閉じる処理。
        /// 必要に応じて派生クラスでオーバーライド可能。
        /// </summary>
        protected virtual void CloseEvent()
        {
            DialogManager.Instance.CloseTopAsync(destroyCancellationToken).Forget();
        }
    }
}