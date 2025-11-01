using System.Threading;
using Alchemy.Inspector;
using Common.UI.Animation;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Common.UI.Loading
{
    /// <summary>
    /// ローディング画面のプレゼンター（シングルトン）。
    /// - 外部コードからの呼び出し窓口
    /// - View の表示/非表示/遷移アニメーションを操作
    /// - 表示中は UI の操作を無効化し、演出が終わると再度制御を戻す
    /// </summary>
    [RequireComponent(typeof(LoadingScreenView))]
    public class LoadingScreenPresenter : SingletonBase<LoadingScreenPresenter>
    {
        [SerializeField, ReadOnly]
        private LoadingScreenView _view;

#if UNITY_EDITOR
        /// <summary>
        /// インスペクタ更新時のチェック。
        /// - View 参照が未設定なら自動取得
        /// </summary>
        private void OnValidate()
        {
            _view ??= GetComponent<LoadingScreenView>();
        }
#endif
        
        /// <summary>
        /// 初期化処理。
        /// - View を初期化し、アニメーションコンポーネントを準備
        /// </summary>
        public void Initialize()
        {
            _view.Initialize();
        }
        
        /// <summary>
        /// ローディング画面をアニメーションで表示。
        /// - 表示中は UI 操作を不可にする
        /// </summary>
        public async UniTask ShowAsync()
        {
            _view.SetInteractable(true);
            await _view.ShowAsync(destroyCancellationToken);
        }
        
        /// <summary>
        /// 任意のワールド座標を中心にして表示（クリック位置などを中心に収束演出）。
        /// </summary>
        public async UniTask ShowAsync(Vector3 centerCirclePosition)
        {
            _view.SetInteractable(true);
            await _view.ShowAsync(centerCirclePosition, destroyCancellationToken);
        }

        /// <summary>
        /// 即時表示（アニメなしで閉じた状態）。
        /// </summary>
        public void Show()
        {
            _view.SetInteractable(true);
            _view.Show();
        }
        
        /// <summary>
        /// アニメーションで非表示。
        /// - 非表示後は UI 操作を不可にする
        /// </summary>
        public async UniTask HideAsync()
        {
            await _view.HideAsync(destroyCancellationToken);
            _view.SetInteractable(false);
        }
        
        /// <summary>
        /// タイトル → ローディング 遷移アニメーション。
        /// </summary>
        public async UniTask TitleToLoadingAsync()
        {
            _view.SetInteractable(true);
            await _view.TitleToLoadingAsync(destroyCancellationToken);
        }
        
        /// <summary>
        /// ローディング → インゲーム 遷移アニメーション。
        /// - 遷移後は UI 操作を不可にする
        /// </summary>
        public async UniTask LoadingToInGameAsync()
        {
            await _view.LoadingToInGameAsync(destroyCancellationToken);
            _view.SetInteractable(false);
        }
    }
}