using System.Collections.Generic;
using System.Threading;
using Common.Audio;
using Common.QRCode;
using Common.Save;
using Common.Scene;
using Common.UI.Animation;
using Common.UI.Dialog;
using Common.UI.Loading;
using Cysharp.Threading.Tasks;
using InGame.Setting;
using OutGame.Network;
using VContainer;

namespace Common.State
{
    public class InGameLoadingState: IState
    {
        private readonly IMasterData _masterData;
        
        private readonly IQRCodeService _qrCodeService;
        
        private readonly IDialogService _dialogService;
        
        private readonly IStateManager _stateManager;
        private readonly IState _toInGameState;
        
        [Inject]
        public InGameLoadingState(
            IMasterData masterData,
            IQRCodeService qrCodeService,
            IDialogService dialogService,
            IStateManager stateManager,
            [Key(StateType.ToInGame)] IState toInGameState)
        {
            _masterData = masterData;
            
            _qrCodeService = qrCodeService;

            _dialogService = dialogService;
            
            _stateManager = stateManager;
            _toInGameState = toInGameState;
        }
        public async UniTask EnterAsync(CancellationToken ct)
        {
            WebsocketManager.Instance.ConnectWebSocket(null).Forget();
            //_qrCodeService.UpdateSprite(await WebsocketManager.Instance.GetFrontUrlAsync());

            if (!_masterData.IsDataFetched)
            {
                await _masterData.FetchDataAsync(ct);
            }

            if (_masterData.IsDataFetched)
            {
                _stateManager.ChangeState(_toInGameState);
            }
            else
            {
                _dialogService.OpenDisplayAsync<IReconnectionDialog>(ct).Forget();
            }
        }
        
        public async UniTask ExitAsync(CancellationToken ct)
        {
            await _dialogService.CloseTopAsync(ct);
        }
    }
}