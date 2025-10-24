using VContainer;
using VContainer.Unity;

namespace Common.UI.Loading
{
    public class LoadingLifeTimeScope: LifetimeScope
    {
        protected override void Configure(IContainerBuilder builder)
        {
            var view = GetComponent<ILoadingScreenView>();
            builder.RegisterComponent(view)
                .As<ILoadingScreenView>()
                .As<IInitializable>();

            builder
                .RegisterEntryPoint<Wiring<ILoadingScreen, LoadingScreenPresenterContext>>()
                .WithParameter(resolver =>
                {
                    return new LoadingScreenPresenterContext
                    {
                        View = resolver.Resolve<ILoadingScreenView>()
                    };
                });
        }
    }
}