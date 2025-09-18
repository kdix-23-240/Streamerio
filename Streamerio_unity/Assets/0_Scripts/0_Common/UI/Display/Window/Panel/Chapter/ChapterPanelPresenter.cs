using System.Threading;
using Alchemy.Inspector;
using Common.UI.Display.Window.Animation;
using Common.UI.Display.Window.Group;
using Common.UI.Guard;
using Cysharp.Threading.Tasks;
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

        private WindowButtonGroup _buttonGroup;
        private BookWindowAnimation _bookWindowAnimation;

        private CancellationTokenSource _cts;

        private Subject<ChapterType> _closeEvent;
        public Subject<ChapterType> CloseEvent => _closeEvent;

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
        /// <param name="buttonGroup"></param>
        /// <param name="bookWindowAnimation"></param>
        public void Initialize(WindowButtonGroup buttonGroup, BookWindowAnimation bookWindowAnimation)
        {
            _view.Initialize();
            
            _currentIndexProp = new ReactiveProperty<int>();

            _buttonGroup = buttonGroup;
            _bookWindowAnimation = bookWindowAnimation;

            _closeEvent = new();
        }

        /// <summary>
        /// 最初のページをアニメーションで開く
        /// </summary>
        /// <param name="ct"></param>
        public async UniTask ShowAsync(CancellationToken ct)
        {
            Bind();
            
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
            Bind();
            
            _view.SetInteractable(true);
            _view.Show();
            
            _currentIndexProp.Value = 0;
            _view.ShowPage(_currentIndex);
        }
        
        /// <summary>
        /// イベント焼き付け
        /// </summary>
        private void Bind()
        {
            _cts = CancellationTokenSource.CreateLinkedTokenSource(destroyCancellationToken);
            
            _buttonGroup.NextButton.ClickEventObservable
                .Subscribe(_ =>
                {
                    OpenNextPage(destroyCancellationToken).Forget();
                }).RegisterTo(_cts.Token);

            _buttonGroup.BackButton.ClickEventObservable
                .Subscribe(_ =>
                {
                    OpenPrePage(destroyCancellationToken).Forget();
                }).RegisterTo(_cts.Token);


            _buttonGroup.CloseButton.ClickEventObservable
                .Subscribe(_ =>
                {
                    _closeEvent.OnNext(_preChapter);
                }).RegisterTo(_cts.Token);
            
            _currentIndexProp
                .Subscribe(_ =>
                {
                    _buttonGroup.BackButton.gameObject.SetActive(_currentIndex > 0);
                    _buttonGroup.NextButton.gameObject.SetActive(_currentIndex < _view.LastPageIndex);
                }).RegisterTo(_cts.Token);
        }
        
        /// <summary>
        /// 開いているページをアニメーションで閉じる
        /// </summary>
        /// <param name="ct"></param>
        public async UniTask HideAsync(CancellationToken ct)
        {
            _cts?.Dispose();
            
            await _buttonGroup.HideAsync(ct);
            await _view.HidePageAsync(_currentIndex, ct);
            
            _view.Hide();
            _view.SetInteractable(false);
        }

        /// <summary>
        /// 開いているページを閉じる
        /// </summary>
        public void Hide()
        {
            _cts?.Dispose();
            
            _buttonGroup.Hide();
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
            ClickGuard.Instance.Guard(true);
            await HideAsync(ct);
            await _bookWindowAnimation.PlayTurnRightAsync(ct);
            await ShowPageAsync(_currentIndex + 1, ct);
            ClickGuard.Instance.Guard(false);
        }

        /// <summary>
        /// 前のページを開く
        /// </summary>
        /// <param name="ct"></param>
        private async UniTask OpenPrePage(CancellationToken ct)
        {
            ClickGuard.Instance.Guard(true);
            await _view.HidePageAsync(_currentIndex, ct);
            await _bookWindowAnimation.PlayTurnLeftAsync(ct);
            await ShowPageAsync(_currentIndex - 1, ct);
            ClickGuard.Instance.Guard(false);
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
            await _buttonGroup.ShowAsync(ct);
        }
    }
}