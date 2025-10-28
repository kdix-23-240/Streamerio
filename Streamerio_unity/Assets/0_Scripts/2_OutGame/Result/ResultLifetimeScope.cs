using Common.Booster;
using Common.State;
using VContainer;
using VContainer.Unity;

namespace OutGame.Result
{
    public class ResultLifetimeScope: LifetimeScope
    {
        protected override void Configure(IContainerBuilder builder)
        {
            base.Configure(builder);
            
            builder.Register<IState, ResultState>(Lifetime.Singleton)
                .Keyed(StateType.Result);
            builder.Register<IState, ToTitleState>(Lifetime.Singleton)
                .Keyed(StateType.ToTitle);
            
            SceneBoosterBinder.Bind(builder, StateType.Result);
        }
    }
}