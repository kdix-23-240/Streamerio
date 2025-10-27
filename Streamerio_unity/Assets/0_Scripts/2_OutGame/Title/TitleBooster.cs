using Common.State;
using VContainer;
using VContainer.Unity;

namespace OutGame.Title
{
    public class TitleBooster: IStartable
    {
        private readonly IStateManager _stateManager;
        private readonly IState _initialState;
        
        public TitleBooster(IStateManager stateManager, [Key(StateType.TitleStart)]IState initialState)
        {
            _stateManager = stateManager;
            _initialState = initialState;
        }
        
        public void Start()
        {
            _stateManager.ChangeState(_initialState);
        }
    }
}