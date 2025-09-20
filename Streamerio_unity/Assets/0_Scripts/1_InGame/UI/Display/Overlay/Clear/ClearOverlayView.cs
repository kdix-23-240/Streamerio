using System.Threading;
using Alchemy.Inspector;
using Common.UI.Display.Overlay;
using Common.UI.Part.Text;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace InGame.UI.Display.Overlay
{
    /// <summary>
    /// クリアのオーバレイView
    /// </summary>
    public class ClearOverlayView: OverlayViewBase
    {
        [SerializeField, LabelText("クリックテキスト")]
        private FlashText _clickText;
        
        public override void Initialize()
        {
            base.Initialize();
            
            _clickText.Initialize();
        }

        public override async UniTask ShowAsync(CancellationToken ct)
        {
            await base.ShowAsync(ct);
            Background.SetInteractable(true);
            _clickText.PlayStartTextAnimation();
        }
        
        public override void Show()
        {
            base.Show();
            Background.SetInteractable(true);
            _clickText.PlayStartTextAnimation();
        }
        
        public override async UniTask HideAsync(CancellationToken ct)
        {
            await base.HideAsync(ct);
            _clickText.StopStartTextAnimation();
        }
        
        public override void Hide()
        {
            base.Hide();
            _clickText.StopStartTextAnimation();
        }
    }
}