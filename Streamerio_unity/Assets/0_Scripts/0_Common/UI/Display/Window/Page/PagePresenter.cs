using System;
using System.Threading;
using Alchemy.Inspector;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Common.UI.Display.Window.Page
{
    /// <summary>
    /// ページのつなぎ役
    /// </summary>
    [RequireComponent(typeof(PageView))]
    public class PagePresenter: MonoBehaviour, IDisplay
    {
        [SerializeField, ReadOnly]
        private PageView _view;

#if UNITY_EDITOR
        protected void OnValidate()
        {
            _view ??= GetComponent<PageView>();
        }
#endif
        
        public void Initialize()
        {
            _view.Initialize();
        }

        public async UniTask ShowAsync(CancellationToken ct)
        {
            await _view.ShowAsync(ct);
        }

        public void Show()
        {
            _view.Show();
        }

        public async UniTask HideAsync(CancellationToken ct)
        {
            await _view.HideAsync(ct);
        }

        public void Hide()
        {
            _view.Hide();
        }
    }
}