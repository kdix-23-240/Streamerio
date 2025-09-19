using System.Collections.Generic;
using System.Linq;
using Alchemy.Inspector;
using UnityEngine;

namespace Common.UI.Display.Overlay
{
    /// <summary>
    /// オーバーレイの表示/非表示の管理
    /// </summary>
    public class OverlayManager: DisplayManagerBase<IOverlay, OverlayManager>
    {
        
    }
    
    public interface IOverlay: IDisplay
    {
        
    }
}