using System.Threading;
using Alchemy.Inspector;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Common.UI.Loading
{
    /// <summary>
    /// ローディング画面のつなぎ役
    /// </summary>
    [RequireComponent(typeof(LoadingScreenView))]
    public class LoadingScreenPresenter: SingletonBase<LoadingScreenPresenter>
    {
        [SerializeField, ReadOnly]
        private LoadingScreenView _view;

#if UNITY_EDITOR
        private void OnValidate()
        {
            _view ??= GetComponent<LoadingScreenView>();
        }
#endif
        
        /// <summary>
        /// 初期化
        /// </summary>
        public void Initialize()
        {
            _view.Initialize();
        }
        
        /// <summary>
        /// 表示アニメーション
        /// </summary>
        public async UniTask ShowAsync()
        {
            _view.SetInteractable(true);
            await _view.ShowAsync(destroyCancellationToken);
        }

        /// <summary>
        /// 表示
        /// </summary>
        public void Show()
        {
            _view.SetInteractable(true);
            _view.Show();
        }
        
        /// <summary>
        /// 非表示アニメーション
        /// </summary>
        public async UniTask HideAsync()
        {
            await _view.HideAsync(destroyCancellationToken);
            _view.SetInteractable(false);
        }
        
        /// <summary>
        /// タイトルからローディングへのアニメーション
        /// </summary>
        public async UniTask TitleToLoadingAsync()
        {
            _view.SetInteractable(true);
            await _view.TitleToLoadingAsync(destroyCancellationToken);
        }
        
        /// <summary>
        /// ローディングからインゲームへのアニメーション
        /// </summary>
        public async UniTask LoadingToInGameAsync()
        {
            await _view.LoadingToInGameAsync(destroyCancellationToken);
            _view.SetInteractable(false);
        }
    }
}