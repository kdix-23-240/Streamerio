using System.Linq;
using UnityEngine;

namespace Common.Component
{
    /// <summary>
    /// コンポーネント操作の共通メソッド
    /// </summary>
    public static class ComponentUtil
    {
        /// <summary>
        /// 子オブジェクトからコンポーネント取得(孫以降は含めない)
        /// </summary>
        /// <param name="parent"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T[] GetComponentsDirectChildren<T>(Transform parent)
            where T : UnityEngine.Component
        {
            return parent.Cast<Transform>()
                .Select(t => t.GetComponent<T>())
                .Where(c => c != null)
                .ToArray();
        }
    }
}