using Alchemy.Inspector;
using Common.UI;
using Common.UI.Part.Text;
using UnityEngine;

namespace InGame.UI.Display.Overlay
{
    /// <summary>
    /// ゲームクリア時のオーバーレイ View。
    /// - プレイヤーにクリックを促すテキストを表示
    /// - Presenter からアニメーションの開始/停止を制御される
    /// </summary>
    public class ClearOverlayView : UIBehaviourBase
    {
        [SerializeField, LabelText("クリックテキスト")]
        private FlashText _clickText;
        /// <summary>
        /// 入力を促すための点滅テキスト
        /// </summary>
        public FlashText ClickText => _clickText;
        
        /// <summary>
        /// 初期化処理。
        /// - クリックテキストを初期化
        /// </summary>
        public override void Initialize()
        {
            base.Initialize();
            _clickText.Initialize();
        }
    }
}