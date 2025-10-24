// ============================================================================
// モジュール概要: 共通ボタンに紐づく識別子を定義し、DI で Presenter を特定できるようにする。
// 外部依存: なし。
// 使用例: ButtonLifetimeScope で ButtonType.Close を指定し、対応する Presenter を解決する。
// ============================================================================

namespace Common.UI.Part.Button
{
    /// <summary>
    /// 共通ボタンの種類を表す列挙型。
    /// </summary>
    public enum ButtonType
    {
        /// <summary>
        /// 汎用的なボタン。
        /// </summary>
        Default = 5,
        /// <summary>
        /// ダイアログを閉じるボタン。
        /// </summary>
        Close = 2,
        /// <summary>
        /// ゲームを再開またはリスタートするボタン。
        /// </summary>
        Restart = 3,
        /// <summary>
        /// タイトル画面へ戻るボタン。
        /// </summary>
        Title = 4,
    }
}
