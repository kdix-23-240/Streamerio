using Alchemy.Inspector;
using Common.UI.Part.Button;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Common.UI.Display.Window.Panel
{
    /// <summary>
    /// メニューパネルの View（見た目担当）。
    /// - スタート / 遊び方 / オプション / クレジット 各ボタンを保持
    /// - 各ボタンの Initialize を呼び出して、共通の初期化処理を適用する
    /// - 実際のボタン押下時の処理は Presenter 側で制御される
    /// </summary>
    public class MenuPagePanelView : UIBehaviour
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

        /// <summary>
        /// 初期化処理。
        /// - 各ボタンの共通初期化を呼び出す
        /// </summary>
        public void Initialize()
        {
            _startButton.Initialize();
            _howToPlayButton.Initialize();
            _optionButton.Initialize();
            _creditButton.Initialize();
        }
    }
}