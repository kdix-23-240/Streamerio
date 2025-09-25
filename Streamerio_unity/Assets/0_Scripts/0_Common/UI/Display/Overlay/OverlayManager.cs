using UnityEngine;

namespace Common.UI.Display.Overlay
{
    /// <summary>
    /// Overlay 系 UI の表示/非表示を管理するマネージャ。
    /// - DisplayManagerBase を継承してスタック制御（Open/Close）を利用可能
    /// - OverlayService を生成して Display 生成・初期化処理を委譲
    /// - シングルトンとしてグローバルからアクセス可能
    /// </summary>
    public class OverlayManager : DisplayManagerBase<OverlayRepositorySO, OverlayManager>
    {
        /// <summary>
        /// Overlay 用の DisplayService をインスタンス化。
        /// リポジトリと親 Transform を受け取り、Overlay 専用のサービスを返す。
        /// </summary>
        protected override IDisplayService InstanceDisplayService(OverlayRepositorySO repository, Transform parent)
        {
            return new OverlayService(repository, parent);
        }
    }
}