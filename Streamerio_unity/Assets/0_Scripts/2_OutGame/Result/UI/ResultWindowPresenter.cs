using Common;
using Common.State;
using Common.UI.Display.Window;
using R3;

namespace OutGame.Result.UI
{
    public interface IResultWindow : IWindow, IAttachable<ResultWindowContext>
    {
    }
    
    /// <summary>
    /// ゲームクリア時のオーバーレイ Presenter。
    /// - 背景クリックでタイトルシーンへ遷移
    /// - クリック誘導テキストのアニメーションを制御
    /// </summary>
    public class ResultWindowPresenter : WindowPresenterBase<IResultWindowView, ResultWindowContext>, IResultWindow
    {
        private IStateManager _stateManager;
        private IState _nextState;

        protected override void Bind()
        {
            base.Bind();
            View.Background.OnClickAsObservable
                .Subscribe(_ => { CloseEvent(); })
                .RegisterTo(GetCt());
        }

        protected override void AttachContext(ResultWindowContext context)
        {
            base.AttachContext(context);

            _stateManager = context.StateManager;
            _nextState = context.NextState;
        }
        
        protected override void CloseEvent()
        {
            _stateManager.ChangeState(_nextState);
        }
    }

    public class ResultWindowContext : WindowContext<IResultWindowView>
    {
        public IStateManager StateManager;
        public IState NextState;
    }
}