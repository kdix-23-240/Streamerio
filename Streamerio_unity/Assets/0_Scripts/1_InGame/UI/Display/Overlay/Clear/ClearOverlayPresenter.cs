using System.Threading;
using Common.Scene;
using Common.UI.Display.Overlay;
using Cysharp.Threading.Tasks;
using R3;
using UnityEngine;
using Alchemy.Inspector;

namespace InGame.UI.Display.Overlay
{
    [RequireComponent(typeof(ClearOverlayView))]
    public class ClearOverlayPresenter: OverlayPresenterBase
    {
        [SerializeField, ReadOnly]
        private ClearOverlayView _view;

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();
            _view ??= GetComponent<ClearOverlayView>();
        }
#endif
        
        public override void Initialize()
        {
            _view.Initialize();
            base.Initialize();
        }
        
        protected override void SetEvent()
        {
            base.SetEvent();
            
            OnClickAsObservable
                .SubscribeAwait( async (_, ct )=>
                {
                    await HideAsync(ct);
                    SceneManager.Instance.LoadSceneAsync(SceneType.Title).Forget();
                }).RegisterTo(destroyCancellationToken);
        }

        public override async UniTask ShowAsync(CancellationToken ct)
        { 
            await base.ShowAsync(ct);
            _view.ClickText.PlayTextAnimation();
        }
        
        public override void Show()
        {
            base.Show();
            _view.ClickText.PlayTextAnimation();
        }
        
        public override async UniTask HideAsync(CancellationToken ct)
        {
            _view.ClickText.StopTextAnimation();
            await base.HideAsync(ct);
        }
        
        public override void Hide()
        {
            _view.ClickText.StopTextAnimation();
            base.Hide();
        }
    }
}