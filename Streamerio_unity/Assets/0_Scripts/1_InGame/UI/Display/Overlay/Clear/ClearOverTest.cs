using InGame.UI.Display.Overlay;
using UnityEngine;

public class ClearOverTest : MonoBehaviour
{
    [SerializeField]
    private ClearOverlayPresenter _clearOverlayPresenter;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    async void Start()
    {
        _clearOverlayPresenter.Initialize();
        _clearOverlayPresenter.Hide();
        await _clearOverlayPresenter.ShowAsync(destroyCancellationToken);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
