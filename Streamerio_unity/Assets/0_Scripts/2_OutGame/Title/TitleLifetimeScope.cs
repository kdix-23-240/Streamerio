using OutGame.Title.UI;
using VContainer;
using VContainer.Unity;

namespace OutGame.Title
{
    public class TitleLifetimeScope: LifetimeScope
    {
        protected override void Configure(IContainerBuilder builder)
        {
            base.Configure(builder);

            builder.RegisterEntryPoint<TitleBooster>();

            builder.Register<ITitleScreen, TitleScreenPresenter>(Lifetime.Singleton)
                .As<IStartable>();
        }
    }
}