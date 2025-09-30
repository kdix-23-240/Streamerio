using Alchemy.Inspector;
using Common.UI.Part.Button;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Common.UI.Display.Window.Panel
{
    public class MenuPanelView: UIBehaviour
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

        public void Initialize()
        {
            _startButton.Initialize();
            _howToPlayButton.Initialize();
            _optionButton.Initialize();
            _creditButton.Initialize();
        }
    }
}