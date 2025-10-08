using System.Linq;
using Alchemy.Inspector;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Common.UI.Display
{
    /// <summary>
    /// Display の取得を提供するリポジトリのインターフェース。
    /// 型パラメータに応じた Display を検索して返す。
    /// </summary>
    public interface IDisplayRepository
    {
        /// <summary>
        /// 指定した型に対応する Display を返す。
        /// 見つからなければ null を返す。
        /// </summary>
        T FindDisplay<T>()
            where T: UIBehaviour, IDisplay;
    }

    /// <summary>
    /// ScriptableObject による Display のリポジトリ基盤。
    /// Unity の Inspector でプレハブを登録し、
    /// 型に応じて取得できるようにする。
    /// </summary>
    public class DisplayRepositorySOBase<T>: ScriptableObject, IDisplayRepository
        where T: UIBehaviour, IDisplay
    {
        [SerializeField, LabelText("UIのプレファブ")]
        private T[] _displays;
        
        public T FindDisplay<T>()
            where T: UIBehaviour, IDisplay
        {
             return _displays.AsEnumerable()
                .FirstOrDefault(x => x != null && x.TryGetComponent<T>(out _)) as T;
        }
    }
}