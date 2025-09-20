using UnityEngine;

namespace Common.UI.Dialog
{
    public class DialogTest: MonoBehaviour
    {
        private async void Start()
        {
            Debug.Log("DialogTest Start");
            DialogManager.Instance.Initialize();
            await DialogManager.Instance.OpenDialogAsync(DialogType.QRCode, destroyCancellationToken);
            Debug.Log("DialogTest End");
        }
    }
}