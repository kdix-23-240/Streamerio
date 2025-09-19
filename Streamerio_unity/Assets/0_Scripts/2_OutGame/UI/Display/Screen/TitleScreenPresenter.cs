using Alchemy.Inspector;
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
            Hide();
            TitleManager.Instance.OpenTitleWindowAsync(destroyCancellationToken).Forget();
        }
    }
}