using System.Threading;
using Alchemy.Inspector;
using Common.UI.Part.Button;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Common.UI.Display.Window.Panel
{
    /// <summary>
    /// 章（Chapter）のパネルの見た目を管理するクラス。
    /// - 複数のページ（PagePanelPresenter）を保持し、表示/非表示を切り替える
    /// - 閉じる/次へ/戻るボタンを提供し、ユーザー操作を受け付ける
    /// - ページ遷移は Presenter 側で制御できるよう公開メソッドを提供
    /// </summary>
    public class ChapterPanelView : UIBehaviourBase
    {
        [SerializeField, LabelText("ページ")]
        private PagePanelPresenter[] _pages;

        [SerializeField, LabelText("表示の透明度")]
        private float _showAlpha = 1f;

        [SerializeField, LabelText("非表示の透明度")]
        private float _hideAlpha = 0f;
        
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
        /// <summary>最後のページ番号（0始まり）</summary>
        public int LastPageIndex => _lastPageIndex;

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();
            
            // Inspector で未設定なら自動で子階層から取得
            if (_pages.Length == 0)
                _pages = GetComponentsInChildren<PagePanelPresenter>(true);
        }
#endif

        /// <summary>
        /// 初期化処理。
        /// - 各種ボタンを初期化
        /// - ページ群を初期化してすべて非表示に設定
        /// - 最後のページ番号をキャッシュ
        /// </summary>
        public override void Initialize()
        {
            base.Initialize();
            
            _closeButton.Initialize();
            _nextButton.Initialize();
            _backButton.Initialize();
            
            _lastPageIndex = _pages.Length - 1;

            foreach (var page in _pages)
            {
                page.Initialize();
                page.Hide();   
            }
        }

        /// <summary>
        /// 全体を表示状態にする（透明度のみ変更）。
        /// </summary>
        public void Show()
        {
            CanvasGroup.alpha = _showAlpha;
        }
        
        /// <summary>
        /// 全体を非表示状態にする（透明度のみ変更）。
        /// </summary>
        public void Hide()
        {
            CanvasGroup.alpha = _hideAlpha;
        }

        /// <summary>
        /// 指定ページをアニメーション付きで表示する。
        /// </summary>
        public async UniTask ShowPageAsync(int index, CancellationToken ct)
        {
            await _pages[index].ShowAsync(ct);
        }

        /// <summary>
        /// 指定ページを即時表示する。
        /// </summary>
        public void ShowPage(int index)
        {
            _pages[index].Show();
        }

        /// <summary>
        /// 指定ページをアニメーション付きで非表示にする。
        /// </summary>
        public async UniTask HidePageAsync(int index, CancellationToken ct)
        {
            await _pages[index].HideAsync(ct);
        }

        /// <summary>
        /// 指定ページを即時非表示にする。
        /// </summary>
        public void HidePage(int index)
        {
            _pages[index].Hide();
        }
    }
}