using System.Threading;
using Common.UI.Animation;
using Common.UI.Display;
using Common.UI.Part.Text;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace OutGame.UI.Window
{
    public class ClearWindowView: DisplayViewBase
    {
        [SerializeField]
        private RankCellView _mostClickerText;
        [SerializeField]
        private RankCellView _bestBadyText;
        [SerializeField]
        private RankCellView _bestVillainText;
        
        [SerializeField]
        private FlashText _flashText;
        
        [SerializeField]
        private Vector2 _initialPosition = new(0, -1000);
        
        [SerializeField]
        private MoveAnimationComponentParam _showAnimParam = new()
        {
            Position = Vector2.zero,
            Duration = 0.2f,
            Ease = DG.Tweening.Ease.InSine,
        };
        [SerializeField]
        private MoveAnimationComponentParam _hideAnimParam = new()
        {
            Position = Vector2.zero,
            Duration = 0.2f,
            Ease = DG.Tweening.Ease.OutSine,
        };
        
        private MoveAnimationComponent _showAnim;
        private MoveAnimationComponent _hideAnim;
        
        public override void Initialize()
        {
            base.Initialize();
            
            _mostClickerText.Initialize();
            _bestBadyText.Initialize();
            _bestVillainText.Initialize();
            
            _flashText.Initialize();
            
            _showAnim = new MoveAnimationComponent(RectTransform, _showAnimParam);
            _hideAnim = new MoveAnimationComponent(RectTransform, _hideAnimParam);
        }

        public override async UniTask ShowAsync(CancellationToken ct)
        {
            RectTransform.anchoredPosition = _initialPosition;
            await _showAnim.PlayAsync(ct);
            
            await _bestBadyText.ShowAsync("勇者", ct);
            await _bestVillainText.ShowAsync("魔王", ct);
            await _mostClickerText.ShowAsync("殿堂入り", ct);
            
            _flashText.PlayStartTextAnimation();
        }

        public override void Show()
        {
            RectTransform.anchoredPosition = _showAnimParam.Position;
        }
        
        public override async UniTask HideAsync(CancellationToken ct)
        {
            RectTransform.anchoredPosition = _showAnimParam.Position;
            await _hideAnim.PlayAsync(ct);
            
            _flashText.StopStartTextAnimation();
        }
        
        public override void Hide()
        {
            RectTransform.anchoredPosition = _hideAnimParam.Position;
            _flashText.StopStartTextAnimation();
        }
    }
}