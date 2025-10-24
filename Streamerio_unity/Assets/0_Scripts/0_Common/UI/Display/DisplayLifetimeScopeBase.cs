using VContainer;
using VContainer.Unity;

namespace Common.UI.Display
{
    public abstract class DisplayLifetimeScopeBase<TDisplay, TPresenter, TView, TContext>: LifetimeScope
        where TDisplay : IDisplay, IAttachable<TContext>
        where TPresenter : TDisplay, IStartable
        where TView : IDisplayView
    {
        protected override void Configure(IContainerBuilder builder)
        {
            base.Configure(builder);
            
            var view = GetComponent<TView>();
            builder.RegisterComponent(view)
                .As<TView>()
                .As<IInitializable>();
            
            BindPresenter(builder);
        }

        protected virtual void BindPresenter(IContainerBuilder builder)
        {
            builder.Register<TDisplay, TPresenter>(Lifetime.Singleton)
                .As<TDisplay>()
                .As<IStartable>();
            
            builder.RegisterEntryPoint<Wiring<TDisplay, TContext>>()
                .WithParameter(resolver => resolver.Resolve<TDisplay>())
                .WithParameter(CreateContext);
        }
        
        protected abstract TContext CreateContext(IObjectResolver resolver);
    }
}