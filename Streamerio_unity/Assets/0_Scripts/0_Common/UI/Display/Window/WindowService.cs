using Common.UI.Display.Window.Book;
using UnityEngine;

namespace Common.UI.Display.Window
{
    /// <summary>
    /// Window 用の DisplayService。
    /// - Repository から UI を生成し、親 Transform に配置する
    /// - Display ごとに適切な初期化処理を実行する
    ///   - BookWindowPresenterBase の場合: Initialize() を特別に呼び出す
    ///   - それ以外の Display: 共通の Initialize() を呼び出す
    /// </summary>
    public class WindowService : DisplayServiceBase
    {
        public WindowService(IDisplayRepository repository, Transform parent) 
            : base(repository, parent)
        {
        }
        
        /// <summary>
        /// Display 生成直後に呼ばれる初期化処理。
        /// - BookWindowPresenterBase であれば BookWindow 用の初期化を行う
        /// - その他の Display であれば通常の Initialize を呼ぶ
        /// </summary>
        protected override TDisplay InitializeDisplay<TDisplay>(TDisplay display)
        {
            switch (display)
            {
                case BookWindowPresenterBase bookWindow:
                    bookWindow.Initialize();   // BookWindow 固有の初期化
                    return bookWindow as TDisplay;
                default:
                    display.Initialize();      // 共通の初期化
                    return display;
            }
        }
    }
}