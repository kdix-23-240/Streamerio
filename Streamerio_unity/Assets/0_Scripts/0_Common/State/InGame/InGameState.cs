using System.Threading;
using Common.Audio;
using Cysharp.Threading.Tasks;
using VContainer;
using ITimer = InGame.UI.Timer.ITimer;

namespace Common.State
{
    public class InGameState: IState
    {
        private readonly IAudioFacade _audioFacade;

        private readonly ITimer _timer;
        
        [Inject]
        public InGameState(IAudioFacade audioFacade, ITimer timer)
        {
            _audioFacade = audioFacade;
            _timer = timer;
        }
        
        public async UniTask EnterAsync(CancellationToken ct)
        {
            _timer.StartCountdownTimer();
            await UniTask.WaitForEndOfFrame(cancellationToken: ct);
        }
        
        public async UniTask ExitAsync(CancellationToken ct)
        {
            _timer.StopCountdownTimer();
            _audioFacade.StopBGMAsync(ct).Forget();
            _audioFacade.StopSEAsync(ct).Forget();
            await UniTask.WaitForEndOfFrame(cancellationToken: ct);
        }
    }
}