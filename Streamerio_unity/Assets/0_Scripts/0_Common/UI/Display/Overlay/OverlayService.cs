using UnityEngine;

namespace Common.UI.Display.Overlay
{
    /// <summary>
    /// Overlay 系の Display を生成・初期化するサービス。
    /// - DisplayServiceBase を継承
    /// - 共通の初期化処理に加え、型ごとにカスタム初期化を追加可能
    /// </summary>
    public class OverlayService : DisplayServiceBase
    {
        public OverlayService(OverlayRepositorySO repository, Transform parent)
            : base(repository, parent)
        {
        }

        /// <summary>
        /// Overlay の初期化処理。
        /// - 共通初期化を実行
        /// - 型ごとの追加初期化があれば switch-case で分岐
        /// </summary>
        protected override TDisplay InitializeDisplay<TDisplay>(TDisplay display)
        {
            switch (display)
            {
                default:
                    display.Initialize();
                    display.Hide();
                    Debug.Log($"[OverlayService] {typeof(TDisplay).Name} を初期化しました");
                    break;
            }

            return display;
        }
    }
}