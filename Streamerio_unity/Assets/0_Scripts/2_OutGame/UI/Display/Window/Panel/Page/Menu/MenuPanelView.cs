using Alchemy.Inspector;
using Common.UI.Display.Window.Panel;
using Common.UI.Part.Button;
using UnityEngine;

namespace OutGame.UI.Display.Window.Panel.Page.Menu
{
    public class MenuPanelView: PagePanelView
    {
        [Header("ボタン")]
        [SerializeField, LabelText("スタート")]
        private TextButton _startButton;
        public TextButton StartButton => _startButton;
        [SerializeField, LabelText("遊び方")]
        private TextButton _howToPlayButton;
        public TextButton HowToPlayButton => _howToPlayButton;
        [SerializeField, LabelText("オプション")]
        private TextButton _optionButton;
        public TextButton OptionButton => _optionButton;
        [SerializeField, LabelText("クレジット")]
        private TextButton _creditButton;
        public TextButton CreditButton => _creditButton;
        [SerializeField, LabelText("閉じるボタン")]
        private TextButton _exitButton;
        public TextButton ExitButton => _exitButton;

        public override void Initialize()
        {
            base.Initialize();
            
            _startButton.Initialize();
            _howToPlayButton.Initialize();
            _optionButton.Initialize();
            _creditButton.Initialize();
            _exitButton.Initialize();
        }
    }
}