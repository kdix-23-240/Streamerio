using Common.UI.Part.Group;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Common.UI.Display.Overlay
{
    public abstract class OverlayLifetimeScopeBase<TOverlay, TPresenter, TView, TContext>: DisplayLifetimeScopeBase<TOverlay, TPresenter, TView, TContext>
        where TOverlay : IOverlay, IAttachable<TContext>
        where TPresenter : TOverlay, IStartable
        where TView : IOverlayView
        where TContext: CommonOverlayContext<TView>
    {
        [SerializeField]
        private CommonUIPartGroup _partGroup;
        
        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterComponent(_partGroup)
                .As<ICommonUIPartGroup>()
                .As<IInitializable>();
            
            base.Configure(builder);
        }
    }
}