using Common.UI.Display;
using UnityEngine;

namespace Common.UI.Dialog
{
    /// <summary>
    /// ダイアログの開閉を管理するマネージャ。
    /// - DisplayManagerBase を継承し、スタック方式で複数のダイアログを制御
    /// - DialogService を通じてダイアログの生成・初期化を行う
    /// - シングルトンとして利用され、任意の場所からダイアログ操作が可能
    /// </summary>
    public class DialogManager : DisplayManagerBase<DialogRepositorySO, DialogManager>
    {
        /// <summary>
        /// Dialog 用の DisplayService を生成する。
        /// - リポジトリからプレハブを取得
        /// - 親 Transform 配下にインスタンス化
        /// - 初期化処理を委譲
        /// </summary>
        protected override IDisplayService InstanceDisplayService(DialogRepositorySO repository, Transform parent)
        {
            return new DialogService(repository, parent);
        }
    }
}