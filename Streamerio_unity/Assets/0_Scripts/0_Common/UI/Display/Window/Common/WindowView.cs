using System.Threading;
using Alchemy.Inspector;
using Common.UI.Animation;
using Common.UI.Display.Background;
using Common.UI.Display.Window.Animation;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

namespace Common.UI.Display.Window
{
    /// <summary>
    /// 共通のウィンドウの見た目
    /// </summary>
    [RequireComponent(typeof(BookWindowAnimation))]
    public class WindowView: DisplayViewBase
    {
        [SerializeField, LabelText("背景"), ReadOnly]
        private DisplayBackground _background;
        [SerializeField, LabelText("本のアニメーション"), ReadOnly]
        private BookWindowAnimation _bookWindowAnimation;
        public BookWindowAnimation BookWindowAnimation => _bookWindowAnimation;
        
        [Header("アニメション")]
        [SerializeField, LabelText("表示アニメーション")]
        private MoveAnimationComponentParam _showAnimParam = new()
        {
            Position = Vector2.zero,
            Duration = 0.2f,
            Ease = Ease.InSine,
        };
        [SerializeField, LabelText("非表示アニメーション")]
        private MoveAnimationComponentParam _hideAnimParam = new()
        {
            Position = Vector2.zero,
            Duration = 0.2f,
            Ease = Ease.OutSine,
        };
        
        private MoveAnimationComponent _showAnim;
        private MoveAnimationComponent _hideAnim;

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();
            
            _background ??= GetComponentInChildren<DisplayBackground>();
            _bookWindowAnimation ??= GetComponent<BookWindowAnimation>();
        }
#endif
        
        public override void Initialize()
        {
            base.Initialize();
            
            _background.Initialize();
            _background.Hide();

            _bookWindowAnimation.Initialize();
            
            _showAnim = new MoveAnimationComponent(RectTransform, _showAnimParam);
            _hideAnim = new MoveAnimationComponent(RectTransform, _hideAnimParam);
        }

        public override async UniTask ShowAsync(CancellationToken ct)
        {
            _background.Show();
            await _showAnim.PlayAsync(ct);
        }

        public override void Show()
        {
            _background.Show();
            RectTransform.anchoredPosition = _showAnimParam.Position;
        }

        public override async UniTask HideAsync(CancellationToken ct)
        {
            await _hideAnim.PlayAsync(ct);
            
            _background.Hide();
        }

        public override void Hide()
        {
            RectTransform.anchoredPosition = _hideAnimParam.Position;
            
            _background.Hide();
        }
    }
}