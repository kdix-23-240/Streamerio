using Common.UI.Display.Window.Book;
using UnityEngine;

namespace Common.UI.Display.Window.Panel
{
    /// <summary>
    /// 章パネル（ChapterPanel）の開閉を管理するマネージャ。
    /// - DisplayManagerBase を継承し、共通の開閉スタック管理を利用
    /// - BookWindowAnimation を受け取り、ChapterService 経由で各パネルに渡す
    /// - 章ごとの Presenter をまとめて制御する窓口
    /// </summary>
    public class ChapterManager : DisplayManagerBase<ChapterRepositorySO, ChapterManager>
    {
        /// <summary>
        /// ページめくりなどの演出を管理するアニメーション
        /// </summary>
        private BookWindowAnimation _bookAnimation;
        
        /// <summary>
        /// 初期化処理。
        /// - BookWindowAnimation を保持
        /// - DisplayManagerBase の Initialize を呼んで共通処理を実行
        /// </summary>
        public void Initialize(BookWindowAnimation bookAnimation)
        {
            _bookAnimation = bookAnimation;
            Initialize();
        }

        /// <summary>
        /// DisplayService を生成。
        /// - ChapterService を返し、BookWindowAnimation を渡す
        /// </summary>
        protected override IDisplayService InstanceDisplayService(ChapterRepositorySO repository, Transform parent)
        {
            return new ChapterService(repository, parent, _bookAnimation);
        }
    }
}