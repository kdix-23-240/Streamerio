using System.Threading;
using Alchemy.Inspector;
using Common.Audio;
using Common.UI.Animation;
using Common.UI.Display;
using Common.UI.Display.Background;
using Common.UI.Part.Button;
using Cysharp.Threading.Tasks;
using R3;
using UnityEngine;
using UnityEngine.Serialization;

namespace Common.UI.Dialog
{
    public interface IDialog: IDisplay
    {
    }
    
    public abstract class DialogPresenterBase<TView>: DisplayPresenterBase<TView>, IDialog
        where TView: DialogViewBase
    {
        protected override void SetEvent()
        {
            View.CloseButton.SetClickEvent(() =>
            {
                CloseEvent();
            });
        }
        
        protected override void Bind()
        {
            base.Bind();
            View.Background.OnClickAsObservable
                .Subscribe(_ =>
                {
                    AudioManager.Instance.PlayAsync(SEType.SNESRPG01, destroyCancellationToken).Forget();
                    CloseEvent();
                }).RegisterTo(destroyCancellationToken);
        }

        /// <summary>
        /// 閉じる時のイベント
        /// </summary>
        protected abstract void CloseEvent();
        
        public override async UniTask ShowAsync(CancellationToken ct)
        {
            await base.ShowAsync(ct);
            View.Background.SetInteractable(true);
            await UniTask.WaitWhile(() => IsShow, cancellationToken: ct);
        }
        
        public override void Show()
        {
            base.Show();
            View.Background.SetInteractable(true);
        }
    }
    
    public abstract class DialogViewBase : DisplayViewBase
    {
        [SerializeField, LabelText("背景")]
        private DisplayBackgroundPresenter _background;
        public DisplayBackgroundPresenter Background => _background;
        [SerializeField, LabelText("動かすオブジェクト")]
        private RectTransform _moveRectTransform;
        
        [SerializeField, LabelText("閉じるボタン")]
        private CommonButton _closeButton;
        public CommonButton CloseButton => _closeButton;

        [Header("アニメーション")]
        [SerializeField, LabelText("表示アニメーション")]
        private PathAnimationComponentParam _showAnimationParam;
        [SerializeField, LabelText("非表示アニメーション")]
        private PathAnimationComponentParam _hideAnimationParam;

        private PathAnimationComponent _showAnimation;
        private PathAnimationComponent _hideAnimation;
        
        public override void Initialize()
        {
            base.Initialize();
            
            _background.Initialize();
            _closeButton.Initialize();

            _showAnimation = new PathAnimationComponent(_moveRectTransform, _showAnimationParam);
            _hideAnimation = new PathAnimationComponent(_moveRectTransform, _hideAnimationParam);
        }
        
        public override async UniTask ShowAsync(CancellationToken ct)
        {
            _moveRectTransform.anchoredPosition = _showAnimationParam.Path[0];
            await _background.ShowAsync(ct);
            await _showAnimation.PlayAsync(ct);
        }
        
        public override void Show()
        {
            _moveRectTransform.anchoredPosition = _showAnimationParam.Path[^1];
            _background.Show();
        }
        
        public override async UniTask HideAsync(CancellationToken ct)
        {
            _moveRectTransform.anchoredPosition = _hideAnimationParam.Path[0];
            await _hideAnimation.PlayAsync(ct);
            await _background.HideAsync(ct);
        }
        
        public override void Hide()
        {
            _moveRectTransform.anchoredPosition = _hideAnimationParam.Path[^1];
            _background.Hide();
        }
    }
}
