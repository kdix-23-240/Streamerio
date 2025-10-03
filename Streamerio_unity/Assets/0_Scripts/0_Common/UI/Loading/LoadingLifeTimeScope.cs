using UnityEngine;
using UnityEngine.Serialization;
using VContainer;
using VContainer.Unity;

namespace Common.UI.Loading
{
    public class LoadingLifeTimeScope: LifetimeScope
    {
        [SerializeField]
        private LoadingScreenView _view;
        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterComponent(_view)
                .AsSelf()
                .As<IStartable>(); 
        }
    }
}