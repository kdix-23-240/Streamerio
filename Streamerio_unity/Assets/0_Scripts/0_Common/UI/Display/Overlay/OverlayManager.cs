using Alchemy.Inspector;
using UnityEngine;

namespace Common.UI.Display.Overlay
{
    /// <summary>
    /// オーバーレイの表示/非表示の管理
    /// </summary>
    public class OverlayManager: DisplayManagerBase<OverlayType, OverlayManager>
    {
        [SerializeField, LabelText("共通のオーバーレイ")]
        private CommonOverlayPresenter _commonOverlay;
        
        protected override IDisplay CreateDisplay(OverlayType type)
        {
            IDisplay display = null;
            switch (type)
            {
                case OverlayType.Common:
                    display = Instantiate(_commonOverlay, Parent);
                    break;
            }

            display?.Initialize();
            return display;
        }
    }
}