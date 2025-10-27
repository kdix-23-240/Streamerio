using Common.UI.Animation;
using Common.UI.Display;
using Common.UI.Display.Background;
using Common.UI.Display.Overlay;
using Common.UI.Display.Window;
using Common.UI.Part.Text;
using TMPro;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace OutGame.Result.UI
{
    public class ResultWindowLifetimeScope: WindowLifetimeScopeBase<IResultWindow, ResultWindowPresenter, IResultWindowView, ResultWindowContext>
    {
        [SerializeField]
        private FlashTextBinder _clickTextBinder;
        
        protected override void Configure(IContainerBuilder builder)
        {
            _clickTextBinder.Bind(builder);
            
            base.Configure(builder);
        }
        
        protected override ResultWindowContext CreateContext(IObjectResolver resolver)
        {
            return new ResultWindowContext
            {
                View = resolver.Resolve<IResultWindowView>(),
            };
        }
    }
}