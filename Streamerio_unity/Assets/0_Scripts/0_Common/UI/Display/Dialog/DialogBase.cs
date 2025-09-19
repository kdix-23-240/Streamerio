using System.Threading;
using Alchemy.Inspector;
using Common.UI.Animation;
using Common.UI.Display;
using Common.UI.Display.Background;
using Common.UI.Part.Button;
using Cysharp.Threading.Tasks;
using UnityEngine;

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
                Debug.Log("Close Button Clicked");
                DialogManager.Instance.ClosePreDialogAsync(destroyCancellationToken).Forget();
            });
        }
    }
    
    public abstract class DialogViewBase : DisplayViewBase
    {
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
            
            _closeButton.Initialize();

            _showAnimation = new PathAnimationComponent(RectTransform, _showAnimationParam);
            _hideAnimation = new PathAnimationComponent(RectTransform, _hideAnimationParam);
        }
        
        public override async UniTask ShowAsync(CancellationToken ct)
        {
            RectTransform.anchoredPosition = _showAnimationParam.Path[0];
            await _showAnimation.PlayAsync(ct);
        }
        
        public override void Show()
        {
            RectTransform.anchoredPosition = _showAnimationParam.Path[^1];
        }
        
        public override async UniTask HideAsync(CancellationToken ct)
        {
            RectTransform.anchoredPosition = _hideAnimationParam.Path[0];
            await _hideAnimation.PlayAsync(ct);
        }
        
        public override void Hide()
        {
            RectTransform.anchoredPosition = _hideAnimationParam.Path[^1];
        }
    }
}
