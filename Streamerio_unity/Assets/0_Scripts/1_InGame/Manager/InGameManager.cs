using Alchemy.Inspector;
using Common;
using Common.Audio;
using Common.Save;
using Common.Scene;
using Common.UI.Display.Overlay;
using Common.UI.Display.Window;
using Common.UI.Loading;
using Cysharp.Threading.Tasks;
using InGame.UI.Displau.Mask;
using InGame.UI.Display.Dialog.QRCode;
using InGame.UI.Display.Overlay;
using InGame.UI.Display.Screen;
using InGame.UI.QRCode;
using UnityEngine;

namespace InGame
{
    public class InGameManager: SingletonBase<InGameManager>
    {
        [SerializeField]private float _timeLimit = 180f;
        [SerializeField, LabelText("プレイヤー")]
        private Transform _playerTransform;
        [SerializeField, LabelText("遊び方ウィンドウ")]
        private WindowPresenter _howToPlayWindow;
        
        [SerializeField, LabelText("QRコードダイアログ")]
        private QRCodeDialogPresenter _qrCodeDialog;
        [SerializeField, LabelText("ゲーム画面")]
        private InGameScreenPresenter _inGameScreen;
        [SerializeField, LabelText("マスク")]
        private InGameMaskView _inGameMaskView;

        private async void Start()
        {
            AudioManager.Instance.PlayAsync(BGMType.singetunoyami, destroyCancellationToken).Forget();
            
            var isPlayed = SaveManager.Instance.LoadPlayed();
            
            _howToPlayWindow.Initialize();
            _howToPlayWindow.Hide();
            
            OverlayManager.Instance.Initialize();
            
            var qrGenerator = new QRCodeSpriteGenerater();

            var url = await WebsocketManager.Instance.GetFrontUrlAsync();
            _inGameScreen.Initialize(qrGenerator.Generate(url), _timeLimit);
            
            _qrCodeDialog.Initialize();
            _qrCodeDialog.SetQRCodeSprite(qrGenerator.Generate(url));
            _qrCodeDialog.Hide();
            
            _inGameMaskView.Hide();
            
            // ゲーム画面表示
            await LoadingScreenPresenter.Instance.LoadingToInGameAsync();
            
            if (!isPlayed)
            {
                await _howToPlayWindow.ShowAsync(destroyCancellationToken);
                SaveManager.Instance.SavePlayed();
            }
            else if(!SaveManager.Instance.IsRetry)
            {
                OpenQRCodeDialog();   
            }
            else
            {
                StartGame();
                SaveManager.Instance.IsRetry = false;
            }
        }

        /// <summary>
        /// QRコードダイアログを開く
        /// </summary>
        public async void OpenQRCodeDialog()
        {
            await _howToPlayWindow.HideAsync(destroyCancellationToken);
            await _qrCodeDialog.ShowAsync(destroyCancellationToken);
        }

        /// <summary>
        /// ゲーム開始
        /// </summary>
        public async void StartGame()
        {
            if (!SaveManager.Instance.IsRetry)
            {
                await _qrCodeDialog.HideAsync(destroyCancellationToken);
            }
            
            _inGameScreen.StartGame(destroyCancellationToken);
            Debug.Log("ゲームスタート");

            await UniTask.Delay(1000, cancellationToken: destroyCancellationToken);
            GameClear();
        }

        /// <summary>
        /// ゲームオーバー
        /// </summary>
        public async void GameOver()
        {
            await LoadingScreenPresenter.Instance.ShowAsync(_playerTransform.position);
            AudioManager.Instance.StopBGM();
            SceneManager.Instance.LoadSceneAsync(SceneType.GameOverScene).Forget();
            Debug.Log("ゲームオーバー");
        }
        
        /// <summary>
        /// ゲームクリア
        /// </summary>
        public async void GameClear()
        {
            await WebsocketManager.Instance.SendWebSocketMessage( "{\"type\": \"game_end\" }" );
            await _inGameMaskView.ShowAsync(_playerTransform.position, destroyCancellationToken);
            await OverlayManager.Instance.OpenAndWaitCloseAsync<ClearOverlayPresenter>(destroyCancellationToken);
            AudioManager.Instance.StopBGM();
            Debug.Log("ゲームクリア");
        }
    }
}