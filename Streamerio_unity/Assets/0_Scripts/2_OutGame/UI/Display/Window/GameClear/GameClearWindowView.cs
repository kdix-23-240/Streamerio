using Common.UI.Part.Text;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace OutGame.UI.Display.Window
{
    /// <summary>
    /// ゲームクリア画面の見た目を管理する View クラス。
    /// - MVP 的なプレイヤー情報を表示（最多クリック・ベストバディ・ベストヴィラン）
    /// - 「クリックして進む」などの FlashText を制御
    /// </summary>
    public class GameClearWindowView : UIBehaviour
    {
        [SerializeField] private TMP_Text _mostClickerText; // 最もクリックしたプレイヤー名
        [SerializeField] private TMP_Text _bestBadyText;    // ベストバディ（仲間）の名前
        [SerializeField] private TMP_Text _bestVillainText; // ベストヴィラン（敵役）の名前

        [SerializeField] private FlashText _clickText;      // 点滅する「クリック促し」テキスト

        /// <summary>
        /// 初期化処理。
        /// - 名前テキストを設定
        /// - FlashText を初期化
        /// </summary>
        public void Initialize(string mostClickerName, string bestBadyName, string bestVillainName)
        {
            _mostClickerText.SetText(mostClickerName);
            _bestBadyText.SetText(bestBadyName);
            _bestVillainText.SetText(bestVillainName);
            
            _clickText.Initialize();
        }
        
        /// <summary>
        /// クリック促しテキストの点滅開始。
        /// </summary>
        public void StartFlash()
        {
            _clickText.PlayTextAnimation();
        }
        
        /// <summary>
        /// クリック促しテキストの点滅停止。
        /// </summary>
        public void StopFlash()
        {
            _clickText.StopTextAnimation();
        }
    }
}