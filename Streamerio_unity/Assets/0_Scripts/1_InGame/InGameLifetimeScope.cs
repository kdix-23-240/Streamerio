using Common.Booster;
using Common.State;
using InGame.Setting;
using InGame.UI.Timer;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace InGame
{
    public class InGameLifetimeScope: LifetimeScope
    {
        [SerializeField]
        private InGameSettingSO _inGameSetting;
        protected override void Configure(IContainerBuilder builder)
        {
            base.Configure(builder);

            builder.RegisterInstance<IInGameSetting>(_inGameSetting);
            
            builder.Register<ITimer, TimerPresenter>(Lifetime.Singleton)
                .As<IStartable>();
            
            builder.Register<IState, InGameStartState>(Lifetime.Singleton)
                .Keyed(StateType.InGameStart);
            builder.Register<IState, FirstPlayState>(Lifetime.Singleton)
                .Keyed(StateType.FirstPlay);
            builder.Register<IState, PlayFromTitleState>(Lifetime.Singleton)
                .Keyed(StateType.PlayFromTitle);
            builder.Register<IState, InGameState>(Lifetime.Singleton)
                .Keyed(StateType.InGame);
            builder.Register<IState, ToGameOverState>(Lifetime.Singleton)
                .Keyed(StateType.ToGameOver);

            SceneBoosterBinder.Bind(builder, StateType.InGameStart);
        }
    }
}