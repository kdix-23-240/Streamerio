using InGame.UI.Display.Overlay;
using OutGame.GameOver.Overlay;
using UnityEngine;

namespace Common.UI.Display.Overlay
{
    /// <summary>
    /// Overlay 系の Display を生成・初期化するサービス。
    /// DisplayServiceBase を継承し、Overlay 専用の初期化処理を追加する。
    /// </summary>
    public class OverlayService : DisplayServiceBase
    {
        public OverlayService(OverlayRepositorySO repository, Transform parent)
            : base(repository, parent)
        {
        }

        /// <summary>
        /// Overlay の初期化処理。
        /// - 型ごとに初期化を行う
        /// </summary>
        protected override TDisplay InitializeDisplay<TDisplay>(TDisplay display)
        {
            // 型ごとの初期化を switch 式で整理
            switch (display)
            {
                case CommonOverlayPresenter overlay:
                    overlay.Initialize();
                    break;
                case GameOverOverlayPresenter overlay:
                    overlay.Initialize();
                    break;
                case ClearOverlayPresenter overlay:
                    overlay.Initialize();
                    break;
                default:
                    Debug.LogError("オーバーレイの初期化に失敗しました。");
                    break;
            }

            return display;
        }
    }
}