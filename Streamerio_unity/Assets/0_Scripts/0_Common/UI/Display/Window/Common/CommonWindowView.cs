using System.Threading;
using Alchemy.Inspector;
using Common.UI.Animation;
using Common.UI.Display.Background;
using Common.UI.Display.Window.Book;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

namespace Common.UI.Display.Window
{
    /// <summary>
    /// 共通のウィンドウの見た目
    /// </summary>
    [RequireComponent(typeof(BookWindowAnimation))]
    public class CommonWindowView: DisplayViewBase
    {
        [SerializeField, ReadOnly]
        private DisplayBackgroundPresenter _background;
        [SerializeField, ReadOnly]
        private BookWindowAnimation _bookAnimation;
        public BookWindowAnimation BookAnimation => _bookAnimation;
        
        [Header("アニメション")]
        [SerializeField, LabelText("表示アニメーション")]
        private MoveAnimationComponentParam _showAnimParam = new()
        {
            AnchoredPosition = Vector2.zero,
            DurationSec = 0.2f,
            Ease = Ease.InSine,
        };
        [SerializeField, LabelText("非表示アニメーション")]
        private MoveAnimationComponentParam _hideAnimParam = new()
        {
            AnchoredPosition = Vector2.zero,
            DurationSec = 0.2f,
            Ease = Ease.OutSine,
        };
        
        private MoveAnimationComponent _showAnim;
        private MoveAnimationComponent _hideAnim;

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();
            
            _bookAnimation ??= GetComponent<BookWindowAnimation>();
            _background ??= GetComponentInChildren<DisplayBackgroundPresenter>();
        }
#endif
        
        public override void Initialize()
        {
            base.Initialize();
            
            _bookAnimation.Initialize();
            
            _background.Initialize();
            _background.Hide();
            
            _showAnim = new MoveAnimationComponent(RectTransform, _showAnimParam);
            _hideAnim = new MoveAnimationComponent(RectTransform, _hideAnimParam);
        }

        public override async UniTask ShowAsync(CancellationToken ct)
        {
            await _background.ShowAsync(ct);
            await _showAnim.PlayAsync(ct);
        }

        public override void Show()
        {
            _background.Show();
            RectTransform.anchoredPosition = _showAnimParam.AnchoredPosition;
        }

        public override async UniTask HideAsync(CancellationToken ct)
        {
            await _hideAnim.PlayAsync(ct);
            await _background.HideAsync(ct);
        }

        public override void Hide()
        {
            RectTransform.anchoredPosition = _hideAnimParam.AnchoredPosition;
            _background.Hide();
        }
    }
}