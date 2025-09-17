using System.Threading;
using Alchemy.Inspector;
using Common.UI.Animation;
using Common.UI.Display.Background;
using Common.UI.Display.Window.Animation;
using Common.UI.Guard;
using Common.UI.Part.Button;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using R3;

namespace Common.UI.Display.Window
{
    public abstract class WindowPresenterBase: DisplayPresenterBase<WindowViewBase>
    {
        protected override void SetEvent()
        {
            base.SetEvent();
            
            View.CloseButton
                .SetClickEvent(() =>
                {
                    Debug.Log("Close Button Clicked");
                });
            
            View.NextButton
                .SetClickEvent(async () =>
                {
                    ClickGuard.Instance.Guard(true);
                    await View.NextPageAsync(destroyCancellationToken);
                    ClickGuard.Instance.Guard(false);
                });
            
            View.PreviousButton
                .SetClickEvent(async () =>
                {
                    ClickGuard.Instance.Guard(true);
                    await View.PreviousPageAsync(destroyCancellationToken);
                    ClickGuard.Instance.Guard(false);
                });
        }

        protected override void Bind()
        {
            base.Bind();
            
            View.Background.OnClickAsObservable
                .Subscribe(_ =>
                {
                   Debug.Log("Background Clicked"); 
                }).RegisterTo(destroyCancellationToken);
        }
    }
    
    public abstract class WindowViewBase: DisplayViewBase
    {
        [SerializeField, LabelText("背景")]
        private DisplayBackground _background;
        public DisplayBackground Background => _background;

        [Header("ボタン")]
        [SerializeField, LabelText("閉じるボタン")]
        private CommonButton _closeButton;

        public CommonButton CloseButton => _closeButton;
        [SerializeField, LabelText("次のページボタン")]
        private CommonButton _nextButton;
        public CommonButton NextButton => _nextButton;
        [SerializeField, LabelText("前のページボタン")]
        private CommonButton _previousButton;
        public CommonButton PreviousButton => _previousButton;
        
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

        [SerializeField, LabelText("本のアニメーション")]
        private BookWindowAnimation _bookWindowAnimation;
        
        [Header("内容のアニメーション")]
        [SerializeField, LabelText("全パーツ(表示順)")]
        private CanvasGroup[] _contentParts;
        [SerializeField, LabelText("1個のパーツの表示アニメーション")]
        private FadeAnimationComponentParam _showContentAnimParam = new()
        {
            Alpha = 1f,
            Duration = 0.1f,
            Ease = Ease.InSine,
        };
        
        [SerializeField, LabelText("パーツの表示アニメーション再生間隔")]
        private float _contentAnimInterval = 0.05f;

        [SerializeField, LabelText("パーツの非表示アニメーション")]
        private FadeAnimationComponentParam _hideContentAnimParam = new()
        {
            Alpha = 0f,
            Duration = 0.2f,
            Ease = Ease.OutSine,
        };
        
        private MoveAnimationComponent _showAnim;
        private MoveAnimationComponent _hideAnim;
        
        private FadeAnimationComponent[] _contentShowAnims;
        private FadeAnimationComponent[] _contentHideAnims;

        public override void Initialize()
        {
            base.Initialize();
            
            _background.Initialize();
            
            _closeButton.Initialize();
            _nextButton.Initialize();
            _previousButton.Initialize();

            _showAnim = new MoveAnimationComponent(RectTransform, _showAnimParam);
            _hideAnim = new MoveAnimationComponent(RectTransform, _hideAnimParam);

            _bookWindowAnimation.Initialize();
            
            int length = _contentParts.Length;
            _contentShowAnims = new FadeAnimationComponent[length];
            _contentHideAnims = new FadeAnimationComponent[length];
            
            for(int i = 0; i < length; i++)
            {
                var part = _contentParts[i];
                _contentShowAnims[i] = new FadeAnimationComponent(part, _showContentAnimParam);
                _contentHideAnims[i] = new FadeAnimationComponent(part, _hideContentAnimParam);
            }
        }

        public override async UniTask ShowAsync(CancellationToken ct)
        {
            _background.Show();
            await _showAnim.PlayAsync(ct);
            await ShowContentAsync(ct);
        }

        public override void Show()
        {
            _background.Show();
            RectTransform.anchoredPosition = _showAnimParam.Position;
            ShowContent();
        }

        public override async UniTask HideAsync(CancellationToken ct)
        {
            await HideContentAsync(ct);
            await _hideAnim.PlayAsync(ct);
            _background.Hide();
        }

        public override void Hide()
        {
            HideContent();
            RectTransform.anchoredPosition = _hideAnimParam.Position;
            _background.Hide();
        }

        /// <summary>
        /// 次のページをめくる
        /// </summary>
        /// <param name="ct"></param>
        public virtual async UniTask NextPageAsync(CancellationToken ct)
        {
            await HideContentAsync(ct);
            await _bookWindowAnimation.PlayTurnRightAsync(ct);
            await ShowContentAsync(ct);
        }
        
        /// <summary>
        /// 前のページをめくる
        /// </summary>
        /// <param name="ct"></param>
        public virtual async UniTask PreviousPageAsync(CancellationToken ct)
        {
            await HideContentAsync(ct);
            await _bookWindowAnimation.PlayTurnLeftAsync(ct);
            await ShowContentAsync(ct);
        }

        /// <summary>
        /// 内容をアニメーションで表示する
        /// </summary>
        /// <param name="ct"></param>
        /// <returns></returns>
        private async UniTask ShowContentAsync(CancellationToken ct)
        {
            foreach (var anim in _contentShowAnims)
            {
                anim.PlayAsync(ct).Forget();
                await UniTask.WaitForSeconds(_contentAnimInterval, cancellationToken: ct);
            }
        }

        /// <summary>
        /// 内容を表示する
        /// </summary>
        private void ShowContent()
        {
            SetPartsInteractive(_showContentAnimParam.Alpha);
        }

        /// <summary>
        /// 内容をアニメーションで非表示にする
        /// </summary>
        /// <param name="ct"></param>
        /// <returns></returns>
        private async UniTask HideContentAsync(CancellationToken ct)
        {
            foreach (var anim in _contentHideAnims)
            {
                anim.PlayAsync(ct).Forget();
            }

            await UniTask.WaitForSeconds(_hideContentAnimParam.Duration, cancellationToken: ct);
        }

        /// <summary>
        /// 内容を非表示にする
        /// </summary>
        private void HideContent()
        {
            SetPartsInteractive(_hideContentAnimParam.Alpha);
        }
        
        /// <summary>
        /// パーツの透明度を全て変える
        /// </summary>
        /// <param name="alpha"></param>
        private void SetPartsInteractive(float alpha)
        {
            foreach (var part in _contentParts)
            {
                part.alpha = alpha;
            }
        }
    }
}