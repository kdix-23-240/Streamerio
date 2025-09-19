using System.Threading;
using Alchemy.Inspector;
using Common.UI.Part.Button;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Common.UI.Display.Window.Panel
{
    /// <summary>
    /// 章のパネルの見た目
    /// </summary>
    public class ChapterPanelView: DisplayViewBase
    {
        [SerializeField, LabelText("ページ")]
        private PagePanelPresenter[] _pages;

        [Header("ボタン")]
        [SerializeField, LabelText("閉じるボタン")]
        private CommonButton _closeButton;
        public CommonButton CloseButton => _closeButton;
        [SerializeField, LabelText("次のページボタン")]
        private CommonButton _nextButton;
        public CommonButton NextButton => _nextButton;
        [SerializeField, LabelText("前のページボタン")]
        private CommonButton _backButton;
        public CommonButton BackButton => _backButton;
        
        private int _lastPageIndex;
        /// <summary>
        /// 最後のページ番号
        /// </summary>
        public int LastPageIndex => _lastPageIndex;

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();
            
            if(_pages.Length == 0)
                _pages = GetComponentsInChildren<PagePanelPresenter>(true);
        }
#endif
        
        public override void Initialize()
        {
            base.Initialize();
            
            _closeButton.Initialize();
            _nextButton.Initialize();
            _backButton.Initialize();
            
            _lastPageIndex = _pages.Length-1;

            foreach (var page in _pages)
            {
                page.Initialize();
                page.Hide();   
            }
        }
        
        public override async UniTask ShowAsync(CancellationToken ct)
        {
            CanvasGroup.alpha = 1f;
        }

        public override void Show()
        {
            CanvasGroup.alpha = 1f;
        }
        
        public override async UniTask HideAsync(CancellationToken ct)
        {
            CanvasGroup.alpha = 0f;
        }
        
        public override void Hide()
        {
            CanvasGroup.alpha = 0f;
        }
        
        /// <summary>
        /// ページをアニメーションで表示
        /// </summary>
        /// <param name="index"></param>
        /// <param name="ct"></param>
        public async UniTask ShowPageAsync(int index, CancellationToken ct)
        {
            await _pages[index].ShowAsync(ct);
        }

        /// <summary>
        /// ページを表示
        /// </summary>
        /// <param name="index"></param>
        public void ShowPage(int index)
        {
            _pages[index].Show();
        }

        /// <summary>
        /// ページをアニメーションで非表示
        /// </summary>
        /// <param name="index"></param>
        /// <param name="ct"></param>
        public async UniTask HidePageAsync(int index, CancellationToken ct)
        {
            await _pages[index].HideAsync(ct);
        }

        /// <summary>
        /// ページを非表示
        /// </summary>
        /// <param name="index"></param>
        public void HidePage(int index)
        {
            _pages[index].Hide();
        }
    }
}