using System.Threading;
using Alchemy.Inspector;
using Common.UI;
using InGame.UI.Timer;
using UnityEngine;
using UnityEngine.UI;

namespace InGame.UI.Display.Screen
{
    public class InGameScreenPresenter: UIBehaviourBase
    {
        [SerializeField, LabelText("QRコード")]
        private Image _qrCodeImage;
        [SerializeField, LabelText("タイマー")]
        private TimerPresenter _timer;

        /// <summary>
        /// 初期化
        /// </summary>
        /// <param name="qrCodeSprite"></param>
        /// <param name="timeLimit"></param>
        public void Initialize(Sprite qrCodeSprite, float timeLimit)
        {
            SetInteractable(false);

            _qrCodeImage.sprite = qrCodeSprite;
        }

        /// <summary>
        /// ゲームを開始
        /// </summary>
        /// <param name="timerCt"></param>
        public void StartGame(CancellationToken timerCt)
        {
            SetInteractable(true);
        }
    }
}