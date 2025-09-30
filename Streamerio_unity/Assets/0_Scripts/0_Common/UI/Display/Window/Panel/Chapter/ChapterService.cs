using Common.UI.Display.Window.Book;
using UnityEngine;

namespace Common.UI.Display.Window.Panel
{
    /// <summary>
    /// 章パネル（ChapterPanel）の生成と初期化を行うサービス。
    /// - DisplayServiceBase を継承し、リポジトリ経由でパネルを生成
    /// - BookWindowAnimation を注入し、ページ遷移アニメーションを各章に適用
    /// </summary>
    public class ChapterService : DisplayServiceBase
    {
        /// <summary>
        /// ページ切り替えアニメーションを管理するコンポーネント
        /// </summary>
        private readonly BookWindowAnimation _animation;
        
        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="repository">章パネルのリポジトリ</param>
        /// <param name="parent">親 Transform</param>
        /// <param name="animation">ページ切替アニメーション</param>
        public ChapterService(IDisplayRepository repository, Transform parent, BookWindowAnimation animation)
            : base(repository, parent)
        {
            _animation = animation;
        }
        
        /// <summary>
        /// Display の初期化処理。
        /// - ChapterPanelPresenterBase を持つ場合は BookWindowAnimation を注入
        /// </summary>
        protected override TDisplay InitializeDisplay<TDisplay>(TDisplay display)
        {
            if (display is ChapterPanelPresenterBase chapterPanel)
            {
                // 章パネルにページ切替アニメーションをセット
                chapterPanel.Initialize(_animation);
            }

            return display;
        }
    }
}