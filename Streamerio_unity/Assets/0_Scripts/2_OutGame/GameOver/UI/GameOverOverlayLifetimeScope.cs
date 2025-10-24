using Common.Scene;
using Common.UI.Display;
using Common.UI.Display.Overlay;
using Common.UI.Loading;
using Common.UI.Part.Button;
using VContainer;

namespace OutGame.UI.GameOver
{
    public class GameOverOverlayLifetimeScope: OverlayLifetimeScopeBase<IGameOverOverlay, GameOverOverlayPresenter, IGameOverOverlayView, GameOverOverlayContext>
    {
        protected override void Configure(IContainerBuilder builder)
        {
            builder.Register<ICommonButton, CommonButtonPresenter>(Lifetime.Singleton)
                .Keyed(ButtonType.Restart);
            
            builder.Register<ICommonButton, CommonButtonPresenter>(Lifetime.Singleton)
                .Keyed(ButtonType.Title);
            
            base.Configure(builder);
        }

        protected override GameOverOverlayContext CreateContext(IObjectResolver resolver)
        {
            return new GameOverOverlayContext
            {
                View = resolver.Resolve<IGameOverOverlayView>(),
                SceneManager = resolver.Resolve<ISceneManager>(),
                LoadingScreen = resolver.Resolve<ILoadingScreen>(),
            };
        }
    }
}