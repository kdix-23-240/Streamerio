using Alchemy.Inspector;
using Common.UI;
using Common.UI.Part.Button;
using UnityEngine;

namespace OutGame.GameOver.Overlay
{
    public class GameOverOverlayView: UIBehaviourBase
    {
        [SerializeField, LabelText("リトライボタン")]
        private TextButton _retryButton;
        public TextButton RetryButton => _retryButton;
        [SerializeField, LabelText("タイトルへ戻るボタン")]
        private TextButton _titleButton;
        public TextButton TitleButton => _titleButton;
        
        public override void Initialize()
        {
            base.Initialize();
            
            Debug.Log("GameOverOverlayView Initialize");
            _retryButton.Initialize();
            _titleButton.Initialize();
        }
    }
}