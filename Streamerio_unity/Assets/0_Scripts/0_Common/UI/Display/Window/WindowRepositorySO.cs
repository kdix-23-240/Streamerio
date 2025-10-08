using UnityEngine;

namespace Common.UI.Display.Window
{
    /// <summary>
    /// Window 用のリポジトリ ScriptableObject。
    /// - プロジェクト内に存在する WindowPresenterBase 派生クラスを保持する
    /// - DisplayManager/DisplayService がここから Window を生成・管理する
    /// - Unity メニューから「SO/UI/WindowRepository」として作成可能
    /// </summary>
    [CreateAssetMenu(fileName = "WindowRepository", menuName = "SO/UI/WindowRepository")]
    public class WindowRepositorySO : DisplayRepositorySOBase<WindowPresenterBase>
    {
        // WindowPresenterBase を管理対象とするだけなので特別な処理は不要。
    }
}