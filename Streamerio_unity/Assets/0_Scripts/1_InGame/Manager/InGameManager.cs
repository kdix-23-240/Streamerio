using Alchemy.Inspector;
using Common;
using Common.Save;
using Common.UI.Display.Window;
using Common.UI.Loading;
using InGame.UI.Display.Dialog.QRCode;
using InGame.UI.Display.Overlay;
using InGame.UI.Display.Screen;
using InGame.UI.QRCode;
using UnityEngine;

namespace InGame
{
    public class InGameManager: SingletonBase<InGameManager>
    {
        [SerializeField] private string _url = "";
        [SerializeField]private float _timeLimit = 180f;
        [SerializeField, LabelText("遊び方ウィンドウ")]
        private WindowPresenter _howToPlayWindow;
        [SerializeField, LabelText("クリアUI")]
        private ClearOverlayPresenter _clearOverlay;
        [SerializeField, LabelText("QRコードダイアログ")]
        private QRCodeDialogPresenter _qrCodeDialog;
        [SerializeField, LabelText("ゲーム画面")]
        private InGameScreenPresenter _inGameScreen;

        private async void Start()
        {
            var isPlayed = SaveManager.Instance.LoadPlayed();
            
            _howToPlayWindow.Initialize();
            _howToPlayWindow.Hide();
            
            _clearOverlay.Initialize();
            _clearOverlay.Hide();
            
            var qrGenerator = new QRCodeSpriteGenerater();
            
            _inGameScreen.Initialize(qrGenerator.Generate(_url), _timeLimit);
            
            _qrCodeDialog.Initialize();
            _qrCodeDialog.SetQRCodeSprite(qrGenerator.Generate(_url));
            _qrCodeDialog.Hide();
            
            await LoadingScreenPresenter.Instance.LoadingToInGameAsync();
            
            if (!isPlayed)
            {
                await _howToPlayWindow.ShowAsync(destroyCancellationToken);
                SaveManager.Instance.SavePlayed();
            }
            else
            {
                OpenQRCodeDialog();   
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
            await _qrCodeDialog.HideAsync(destroyCancellationToken);
            _inGameScreen.StartGame(destroyCancellationToken);
            Debug.Log("ゲームスタート");
        }
    }
}