using Alchemy.Inspector;
using Common.Audio;
using Common.UI.Display;
using Cysharp.Threading.Tasks;
using OutGame.Title;
using UnityEngine;
using UnityEngine.EventSystems;

namespace OutGame.UI.Display.Screen
{
    /// <summary>
    /// タイトルのスクリーンのつなぎ役
    /// </summary>
    [RequireComponent(typeof(TitleScreenView))]
    public class TitleScreenPresenter: DisplayPresenterBase<TitleScreenView>, IPointerClickHandler
    {
        public void OnPointerClick(PointerEventData eventData)
        {
            AudioManager.Instance.PlayAsync(SEType.NESRPG0112, destroyCancellationToken).Forget();
            Hide();
            TitleManager.Instance.OpenMainMenuAsync(destroyCancellationToken).Forget();
        }
    }
}