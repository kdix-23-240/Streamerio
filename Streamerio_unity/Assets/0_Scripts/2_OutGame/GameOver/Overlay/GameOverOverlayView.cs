using Alchemy.Inspector;
using Common.UI;
using Common.UI.Part.Button;
using UnityEngine;

namespace OutGame.GameOver.Overlay
{
    /// <summary>
    /// ゲームオーバー画面の View。
    /// - Retry ボタン
    /// - Title ボタン
    /// の参照と初期化を担当する
    /// </summary>
    public class GameOverOverlayView : UIBehaviourBase
    {
        [SerializeField, LabelText("リトライボタン")]
        private TextButton _retryButton;
        /// <summary>ゲームを再開するためのボタン</summary>
        public TextButton RetryButton => _retryButton;
        
        [SerializeField, LabelText("タイトルへ戻るボタン")]
        private TextButton _titleButton;
        /// <summary>タイトル画面へ戻るためのボタン</summary>
        public TextButton TitleButton => _titleButton;
        
        /// <summary>
        /// 初期化処理。
        /// - Retry ボタンと Title ボタンを初期化
        /// </summary>
        public override void Initialize()
        {
            base.Initialize();
            
            _retryButton.Initialize();
            _titleButton.Initialize();
        }
    }
}