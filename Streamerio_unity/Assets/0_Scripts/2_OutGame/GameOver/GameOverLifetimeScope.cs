using Common.Booster;
using Common.State;
using VContainer;
using VContainer.Unity;

namespace OutGame.GameOver
{
    public class GameOverLifetimeScope: LifetimeScope
    {
        protected override void Configure(IContainerBuilder builder)
        {
            builder.Register<IState, GameOverState>(Lifetime.Singleton)
                .Keyed(StateType.GameOver);
            builder.Register<IState, RestartState>(Lifetime.Singleton)
                .Keyed(StateType.Restart);
            builder.Register<IState, ToTitleState>(Lifetime.Singleton)
                .Keyed(StateType.ToTitle);
            
            SceneBoosterBinder.Bind(builder, StateType.GameOver);
        }
    }
}