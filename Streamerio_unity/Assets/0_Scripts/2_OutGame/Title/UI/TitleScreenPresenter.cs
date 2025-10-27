using Common;
using Common.UI.Click;
using Common.UI.Display;
using Common.UI.Display.Window;
using Common.UI.Display.Window.Book;
using R3;

namespace OutGame.Title.UI
{
	public interface ITitleScreen: IDisplay, IAttachable<TitleScreenContext>
	{
	}
	
	public class TitleScreenPresenter : DisplayPresenterBase<ITitleScreenView, TitleScreenContext>, ITitleScreen
	{
		private IClickEventBinder _clickEventBinder;
		private IWindowService _windowService;

		protected override void AttachContext(TitleScreenContext context)
		{
			View = context.View;
			_clickEventBinder = context.ClickEventBinder;
			_windowService = context.WindowService;
		}

		protected override void Bind()
		{
			base.Bind();

			_clickEventBinder.BindClickEvent();
			_clickEventBinder.ClickEvent
				.SubscribeAwait(async (_, ct) =>
				{
					await HideAsync(ct);
					await _windowService.OpenAndWaitCloseAsync<IBookWindow>(ct);
					await ShowAsync(ct);
				}, AwaitOperation.Drop)
				.RegisterTo(GetCt());
		}
	}


	public class TitleScreenContext
	{
		public ITitleScreenView View;
		public IClickEventBinder ClickEventBinder;
		public IWindowService WindowService;
	}

}