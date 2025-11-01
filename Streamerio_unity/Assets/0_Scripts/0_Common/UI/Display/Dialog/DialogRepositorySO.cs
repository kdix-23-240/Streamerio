using Common.UI.Display;
using UnityEngine;

namespace Common.UI.Dialog
{
    /// <summary>
    /// ダイアログ用の Presenter プレハブを管理するリポジトリ。
    /// - ScriptableObject として生成され、インスペクタでプレハブを登録可能
    /// - DisplayService/Manager から参照され、DialogPresenter を検索・取得する役割を担う
    /// </summary>
    [CreateAssetMenu(fileName = "DialogRepository", menuName = "SO/UI/DialogRepository")]
    public class DialogRepositorySO : DisplayRepositorySOBase<DialogPresenterBase> { }
}