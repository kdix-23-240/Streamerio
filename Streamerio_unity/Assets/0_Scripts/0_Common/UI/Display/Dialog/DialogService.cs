using Common.UI.Display;
using UnityEngine;

namespace Common.UI.Dialog
{
    /// <summary>
    /// Dialog 系の Display を生成・初期化するサービス。
    /// - DisplayServiceBase を継承し、DialogPresenter の生成処理を担う
    /// - 必要に応じて型ごとの初期化を追加可能
    /// </summary>
    public class DialogService : DisplayServiceBase
    {
        public DialogService(IDisplayRepository repository, Transform parent) 
            : base(repository, parent) {}

        /// <summary>
        /// Display 生成後の初期化処理。
        /// - 現状は全ての Dialog に対して共通の初期化を行う
        /// - 特定の型ごとの初期化が必要になれば switch-case に追加
        /// </summary>
        protected override TDisplay InitializeDisplay<TDisplay>(TDisplay display)
        {
            switch (display)
            {
                default:
                    // 共通初期化
                    display.Initialize();
                    Debug.Log($"[DialogService] {typeof(TDisplay).Name} を初期化しました");
                    break;
            }

            return display;
        }
    }
}