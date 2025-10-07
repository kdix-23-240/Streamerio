using Common.Save;
using VContainer;
using VContainer.Unity;

namespace Common
{
    public class GlobalLifeTimeScope: LifetimeScope
    {
        protected override void Configure(IContainerBuilder builder)
        {
            builder.Register<IVolumeSaveFacade, IPlayDataSaveFacade, SaveManager>(Lifetime.Singleton);
        }
    }
}