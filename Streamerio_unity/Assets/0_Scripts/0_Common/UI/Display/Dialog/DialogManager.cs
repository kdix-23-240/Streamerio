using System.Collections.Generic;
using System.Threading;
using Alchemy.Inspector;
using Common.UI.Display;
using Common.UI.Display.Background;
using Cysharp.Threading.Tasks;
using InGame.UI.Display.Dialog.QRCode;
using R3;
using UnityEngine;

namespace Common.UI.Dialog
{
    /// <summary>
    /// ダイアログの開閉を管理
    /// </summary>
    public class DialogManager: SingletonBase<DialogManager>
    {
        [SerializeField, LabelText("背景")]
        private DisplayBackgroundPresenter _background;

        [SerializeField, LabelText("ダイアログの親")]
        private Transform _dialogParent;
        [SerializeField]
        private QRCodeDialogPresenter _qrCodeDialogPrefab;
        
        private Dictionary<DialogType,IDisplay> _dialogDict;
        
        private DialogType _currentDialogType = DialogType.None;
        
        public IDisplay CurrentDialog => _dialogDict.ContainsKey(_currentDialogType) ? _dialogDict[_currentDialogType] : null;
        
        public void Initialize()
        {
            _dialogDict = new Dictionary<DialogType, IDisplay>();
            
            _background.Initialize();
            _background.Hide();

            Bind();
        }

        /// <summary>
        /// 焼き付け
        /// </summary>
        private void Bind()
        {
            _background.OnClickAsObservable
                .Subscribe(_ =>
                {
                    ClosePreDialogAsync(destroyCancellationToken).Forget();
                }).RegisterTo(destroyCancellationToken);
        }

        /// <summary>
        /// ダイアログを開く
        /// </summary>
        /// <param name="type"></param>
        /// <param name="ct"></param>
        public async UniTask OpenDialogAsync(DialogType type, CancellationToken ct)
        {
            IDisplay dialog;
            if(!_dialogDict.TryGetValue(type, out dialog))
            {
                switch (type)
                {
                    case DialogType.QRCode:
                        QRCodeDialogPresenter newDialog = Instantiate(_qrCodeDialogPrefab, _dialogParent);
                        newDialog.Initialize();
                        dialog = newDialog;
                        break;
                }
                
                dialog.Hide();
                _dialogDict.Add(type, dialog);
            }
            
            _currentDialogType = type;
            
            _background.ShowAsync(ct).Forget();
            await dialog.ShowAsync(ct);
            _background.SetInteractable(true);
            await UniTask.WaitWhile(() => dialog.IsShow, cancellationToken: ct);
        }
        
        /// <summary>
        /// 前のダイアログを閉じる
        /// </summary>
        /// <param name="ct"></param>
        public async UniTask ClosePreDialogAsync(CancellationToken ct)
        {
            Debug.Log($"ClosePreDialog: {_currentDialogType}");
            if(_currentDialogType == DialogType.None)
                return;

            _background.HideAsync(ct).Forget();
            await _dialogDict[_currentDialogType].HideAsync(ct);
        }
    }
}