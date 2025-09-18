using System.Threading;
using Alchemy.Inspector;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Common.UI.Display.Window.Panel
{
    /// <summary>
    /// 章のパネルの見た目
    /// </summary>
    public class ChapterPanelView: UIBehaviourBase
    {
        [SerializeField, LabelText("ページ")]
        private PagePanelPresenter[] _pages;

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
            
            _lastPageIndex = _pages.Length-1;

            foreach (var page in _pages)
            {
                page.Initialize();
                page.Hide();   
            }
        }

        /// <summary>
        /// 表示
        /// </summary>
        public void Show()
        {
            CanvasGroup.alpha = 1f;
        }
        
        /// <summary>
        /// 非表示
        /// </summary>
        public void Hide()
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