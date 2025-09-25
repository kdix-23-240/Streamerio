using System.Threading;
using Alchemy.Inspector;
using Common.UI;
using Common.UI.Display.Overlay;
using Common.UI.Part.Text;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace InGame.UI.Display.Overlay
{
    /// <summary>
    /// クリアのオーバレイView
    /// </summary>
    public class ClearOverlayView: UIBehaviourBase
    {
        [SerializeField, LabelText("クリックテキスト")]
        private FlashText _clickText;
        public FlashText ClickText => _clickText;
        
        public override void Initialize()
        {
            base.Initialize();
            
            _clickText.Initialize();
        }
    }
}