using Alchemy.Inspector;
using Common;
using Common.Audio;
using Common.Save;
using Common.Scene;
using Common.UI.Dialog;
using Common.UI.Display.Overlay;
using Common.UI.Display.Window;
using Common.UI.Loading;
using Cysharp.Threading.Tasks;
using InGame.UI.Display.Dialog.QRCode;
using InGame.UI.Display.Overlay;
using InGame.UI.Display.Screen;
using InGame.UI.QRCode;
using InGame.UI.Window;
using UnityEngine;
using UnityEngine.Serialization;

namespace InGame
{
    public class InGameManager: SingletonBase<InGameManager>
    {
        [SerializeField]private float _timeLimit = 180f;
        [SerializeField, LabelText("プレイヤー")]
        private Transform _playerTransform;
        
        [SerializeField, LabelText("ゲーム画面")]
        private InGameScreenPresenter _inGameScreen;

        private async void Start()
        {
            AudioManager.Instance.PlayAsync(BGMType.singetunoyami, destroyCancellationToken).Forget();
            
            var isPlayed = SaveManager.Instance.LoadPlayed();
            
            var url = await WebsocketManager.Instance.GetFrontUrlAsync();
            await QRCodeSpriteGenerater.InitializeSprite(url);
            
            _inGameScreen.Initialize(QRCodeSpriteGenerater.GetQRCodeSprite(), _timeLimit);
            
            WindowManager.Instance.Initialize();
            
            OverlayManager.Instance.Initialize();
            
            DialogManager.Instance.Initialize();
            
            // ゲーム画面表示
            await LoadingScreenPresenter.Instance.LoadingToInGameAsync();
            
            if (!isPlayed)
            {
                await WindowManager.Instance.OpenAndWaitCloseAsync<InGameBookWindowPresenter>(destroyCancellationToken);
                SaveManager.Instance.SavePlayed();
                OpenQRCodeDialog();
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
            await DialogManager.Instance.OpenAndWaitCloseAsync<QRCodeDialogPresenter>(destroyCancellationToken);
            StartGame();
        }

        /// <summary>
        /// ゲーム開始
        /// </summary>
        public void StartGame()
        {
            _inGameScreen.StartGame(destroyCancellationToken);
            Debug.Log("ゲームスタート");
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
            await OverlayManager.Instance.OpenAndWaitCloseAsync<ClearOverlayPresenter>(destroyCancellationToken);
            AudioManager.Instance.StopBGM();
            Debug.Log("ゲームクリア");
        }
    }
}