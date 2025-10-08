using UnityEngine;

namespace Common.UI.Display.Window.Panel
{
    /// <summary>
    /// 章パネルのリポジトリ (ScriptableObject)。
    /// - ChapterPanelPresenterBase を保持し、DisplayService 経由で参照される
    /// - 章ごとの Presenter をまとめて管理し、生成時に利用する
    /// </summary>
    [CreateAssetMenu(fileName = "ChapterRepositorySO", menuName = "SO/UI/ChapterRepositorySO")]
    public class ChapterRepositorySO : DisplayRepositorySOBase<ChapterPanelPresenterBase>
    {
    }
}