using System.Threading;
using Alchemy.Inspector;
using Common.UI.Display.Window.Animation;
using Cysharp.Threading.Tasks;
using OutGame.Title;
using R3;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Common.UI.Display.Window.Panel
{
    /// <summary>
    /// 章のパネルの繋ぎ役
    /// </summary>
    [RequireComponent(typeof(ChapterPanelView))]
    public class ChapterPanelPresenter: UIBehaviour
    {
        [SerializeField, ReadOnly]
        private ChapterPanelView _view;

        [SerializeField, LabelText("前のチャプター")]
        private ChapterType _preChapter;
        
        private ReactiveProperty<int> _currentIndexProp;
        private int _currentIndex => _currentIndexProp.Value;

        private BookWindowAnimation _bookWindowAnimation;

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();

            _view ??= GetComponent<ChapterPanelView>();
        }
#endif

        /// <summary>
        /// 初期化
        /// </summary>
        /// <param name="bookWindowAnimation"></param>
        public void Initialize(BookWindowAnimation bookWindowAnimation)
        {
            _view.Initialize();
            
            _currentIndexProp = new ReactiveProperty<int>();

            _bookWindowAnimation = bookWindowAnimation;

            SetEvnet();
            Bind();
        }

        /// <summary>
        /// イベント設定
        /// </summary>
        private void SetEvnet()
        {
            _view.NextButton.SetClickEvent(()=> OpenNextPage(destroyCancellationToken).Forget());
            _view.BackButton.SetClickEvent(()=> OpenPrePage(destroyCancellationToken).Forget());
            _view.CloseButton.SetClickEvent(() =>
            {
                if (_preChapter == ChapterType.None)
                {
                    TitleManager.Instance.ShowTitleAsync(destroyCancellationToken).Forget();
                }
                else
                {
                    ChapterManager.Instance.OpenChapterAsync(_preChapter, destroyCancellationToken).Forget();
                }
            });
        }
        
        /// <summary>
        /// イベント焼き付け
        /// </summary>
        private void Bind()
        {
            _currentIndexProp
                .Subscribe(_ =>
                {
                    _view.BackButton.gameObject.SetActive(_currentIndex > 0);
                    _view.NextButton.gameObject.SetActive(_currentIndex < _view.LastPageIndex);
                }).RegisterTo(destroyCancellationToken);
        }

        /// <summary>
        /// 最初のページをアニメーションで開く
        /// </summary>
        /// <param name="ct"></param>
        public async UniTask ShowAsync(CancellationToken ct)
        {
            _view.SetInteractable(true);
            _view.Show();
            
            _currentIndexProp.Value = 0;
            await ShowPageAsync(_currentIndex, ct);
        }

        /// <summary>
        /// 最初のページを開く
        /// </summary>
        public void Show()
        {
            _view.SetInteractable(true);
            _view.Show();
            
            _currentIndexProp.Value = 0;
            _view.ShowPage(_currentIndex);
        }
        
        /// <summary>
        /// 開いているページをアニメーションで閉じる
        /// </summary>
        /// <param name="ct"></param>
        public async UniTask HideAsync(CancellationToken ct)
        {
            await _view.HidePageAsync(_currentIndex, ct);
            
            _view.Hide();
            _view.SetInteractable(false);
        }

        /// <summary>
        /// 開いているページを閉じる
        /// </summary>
        public void Hide()
        {
            _view.HidePage(_currentIndex);
            
            _view.Hide();
            _view.SetInteractable(false);
        }
        
        /// <summary>
        /// 次のページを開く
        /// </summary>
        /// <param name="ct"></param>
        private async UniTask OpenNextPage(CancellationToken ct)
        {
            _view.SetInteractable(false);
            
            _view.HidePageAsync(_currentIndex, ct).Forget();
            await _bookWindowAnimation.PlayTurnRightAsync(ct);
            await ShowPageAsync(_currentIndex + 1, ct);
            
            _view.SetInteractable(true);
        }

        /// <summary>
        /// 前のページを開く
        /// </summary>
        /// <param name="ct"></param>
        private async UniTask OpenPrePage(CancellationToken ct)
        {
            _view.SetInteractable(false);
            
            _view.HidePageAsync(_currentIndex, ct).Forget();
            await _bookWindowAnimation.PlayTurnLeftAsync(ct);
            await ShowPageAsync(_currentIndex - 1, ct);
            
            _view.SetInteractable(true);
        }

        /// <summary>
        /// ページを表示する
        /// </summary>
        /// <param name="nextIndex"></param>
        /// <param name="ct"></param>
        private async UniTask ShowPageAsync(int nextIndex, CancellationToken ct)
        {
            _currentIndexProp.Value = Mathf.Clamp(nextIndex, 0, _view.LastPageIndex);
            await _view.ShowPageAsync(_currentIndex, ct);
        }
    }
}