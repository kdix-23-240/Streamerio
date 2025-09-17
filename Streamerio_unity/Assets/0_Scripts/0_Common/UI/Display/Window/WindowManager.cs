using OutGame.UI.Display.Window.Credit;
using UnityEditor;
using UnityEngine;

namespace Common.UI.Display.Window
{
    public class WindowManager: DisplayManagerBase<WindowType, WindowManager>
    {
        [SerializeField, Header("クレジットウィンドウ")]
        private CreditWindowPresenter _creditWindow;
        
        protected override IDisplay CreateDisplay(WindowType type)
        {
            IDisplay display = null;
            display = type switch
                {
                    WindowType.Credit => Instantiate(_creditWindow, Parent),
                    _ => null
                };

            display?.Initialize();
            return display;
        }
    }
}