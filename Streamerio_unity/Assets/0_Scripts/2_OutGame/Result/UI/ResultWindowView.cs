using System.Threading;
using Common.UI.Animation;
using Common.UI.Display.Window;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using VContainer;

namespace OutGame.Result.UI
{
    /// <summary>
    /// ゲームクリア時のオーバーレイ View。
    /// - プレイヤーにクリックを促すテキストを表示
    /// - Presenter からアニメーションの開始/停止を制御される
    /// </summary>
    public class ResultWindowView : WindowViewBase, IResultWindowView
    {
        [SerializeField]
        private TMP_Text _allText;
        [SerializeField]
        private TMP_Text _enemyText;
        [SerializeField]
        private TMP_Text _skillText;
        
        private IUIAnimation _clickTextAnimation;
        
        [Inject]
        public void Construct([Key(AnimationType.FlashText)] IUIAnimation clickTextAnimation)
        {
            _clickTextAnimation = clickTextAnimation;
        }

        /// <summary>
        /// アニメーション付き表示。
        /// - クリック誘導テキストのアニメーションを開始
        /// </summary>
        public override async UniTask ShowAsync(CancellationToken ct)
        { 
            await base.ShowAsync(ct);
            
            if(WebsocketManager.Instance.GameEndSummary != null)
            {
                var summary = WebsocketManager.Instance.GameEndSummary;
                _allText.text = summary.All;
                _enemyText.text = summary.Enemy;
                _skillText.text = summary.Skill;
            }
            
            _clickTextAnimation.PlayAsync(destroyCancellationToken).Forget();
        }
        
        /// <summary>
        /// 即時表示。
        /// - クリック誘導テキストのアニメーションを開始
        /// </summary>
        public override void Show()
        {
            base.Show();
            _clickTextAnimation.PlayAsync(destroyCancellationToken).Forget();
        }
        
        /// <summary>
        /// アニメーション付き非表示。
        /// - クリック誘導テキストのアニメーションを停止
        /// </summary>
        public override async UniTask HideAsync(CancellationToken ct)
        {
            _clickTextAnimation.Skip();
            await base.HideAsync(ct);
        }
        
        /// <summary>
        /// 即時非表示。
        /// - クリック誘導テキストのアニメーションを停止
        /// </summary>
        public override void Hide()
        {
            _clickTextAnimation.Skip();
            base.Hide();
        }
        
        public void SkipShowAnimation()
        {
            ShowAnim.Skip();
            ShowPartsAnim.Skip();
        }
    }
    
    public interface IResultWindowView : IWindowView
    {
        void SkipShowAnimation();
    }
}