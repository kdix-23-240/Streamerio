namespace Common
{
    /// <summary>
    /// 特定の <typeparamref name="TContext"/> を用いて
    /// オブジェクト同士の接続（Attach）および切断（Detach）を行うためのインターフェース。
    /// </summary>
    /// <typeparam name="TContext">
    /// Attach 時に渡されるコンテキスト情報の型。
    /// 複数の依存関係や初期化用パラメータなどをまとめる用途に使用します。
    /// </typeparam>
    public interface IAttachable<TContext>
    {
        /// <summary>
        /// 指定された <paramref name="context"/> を用いて対象を接続します。
        /// View とのバインドやイベント購読、初期設定などをここで行います。
        /// </summary>
        /// <param name="context">接続に必要なコンテキスト情報</param>
        void Attach(TContext context);

        /// <summary>
        /// 現在の接続を解除します。
        /// イベント購読の解除や参照のクリアなど、クリーンアップ処理を行います。
        /// </summary>
        void Detach();
    }
}