using System;
using Alchemy.Inspector;
using Common;
using Common.UI.Loading;
using Cysharp.Threading.Tasks;
using InGame.UI.Display.Overlay;
using UnityEngine;

namespace InGame
{
    public class InGameManager: SingletonBase<InGameManager>
    {
        [SerializeField, LabelText("クリアUI")]
        private ClearOverlayPresenter _clearOverlay;

        private void Start()
        {
            _clearOverlay.Initialize();
            _clearOverlay.Hide();
            
            LoadingScreenPresenter.Instance.LoadingToInGameAsync().Forget();
        }
    }
}