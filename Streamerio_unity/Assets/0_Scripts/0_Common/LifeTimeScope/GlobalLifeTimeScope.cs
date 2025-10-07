using Common.Audio;
using Common.Save;
using UnityEngine;
using UnityEngine.Serialization;
using VContainer;
using VContainer.Unity;

namespace Common
{
    public class GlobalLifeTimeScope: LifetimeScope
    {
        [SerializeField]
        private AudioMediator _audioMediator;
        protected override void Configure(IContainerBuilder builder)
        {
            builder.Register<IVolumeSaveFacade, IPlayDataSaveFacade, SaveManager>(Lifetime.Singleton);
            
            builder.RegisterComponent(_audioMediator);
            builder.Register<IAudioFacade, AudioFacade>(Lifetime.Singleton);
        }
    }
}