using Alchemy.Inspector;
using Common.Save;
using Common.Scene;
using Common.UI.Display.Overlay;
using Common.UI.Loading;
using Cysharp.Threading.Tasks;
using R3;
using UnityEngine;

namespace OutGame.GameOver.Overlay
{
    [RequireComponent(typeof(GameOverOverlayView))]
    public class GameOverOverlayPresenter: OverlayPresenterBase
    {
        [SerializeField, ReadOnly]
        private GameOverOverlayView _view;

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();
            _view ??= GetComponent<GameOverOverlayView>();
        }
#endif

        public override void Initialize()
        {
            _view.Initialize();
            base.Initialize();
        }

        protected override void Bind()
        {
            base.Bind();
            _view.RetryButton.OnClickAsObservable
                .SubscribeAwait(async (_, ct) =>
                {
                    await LoadingScreenPresenter.Instance.ShowAsync();
                    SaveManager.Instance.IsRetry = true;
                    SceneManager.Instance.LoadSceneAsync(SceneType.GameScene).Forget();
                }).RegisterTo(destroyCancellationToken);
            
            _view.TitleButton.OnClickAsObservable
                .SubscribeAwait(async (_, ct) =>
                {
                    await LoadingScreenPresenter.Instance.ShowAsync();
                    SceneManager.Instance.LoadSceneAsync(SceneType.Title).Forget();
                }).RegisterTo(destroyCancellationToken);
        }
    }
}