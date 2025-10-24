// ============================================================================
// モジュール概要: ローディング画面の Presenter 層を定義し、外部 API と View の橋渡しを担う。
// 外部依存: Cysharp.Threading.Tasks（UniTask）、UnityEngine（Vector3）、IAttachable（依存注入契約）。
// 使用例: シーン遷移時に ILoadingScreen.OpenAsync 系 API を呼び出し、統一されたローディング演出を再生する。
// ============================================================================

using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Common.UI.Loading
{
    /// <summary>
    /// ローディング画面の公開 API 契約。
    /// <para>
    /// 【理由】Presenter への依存を抽象化し、DI 経由でモジュール境界を明確にする。
    /// </para>
    /// </summary>
    public interface ILoadingScreen : IAttachable<LoadingScreenPresenterContext>
    {
        /// <summary>
        /// 【目的】標準的なアニメーションでローディング画面を表示する。
        /// </summary>
        UniTask ShowAsync();

        /// <summary>
        /// 【目的】任意座標を中心としたイリス演出でローディング画面を表示する。
        /// </summary>
        UniTask ShowAsync(Vector3 centerCirclePosition);

        /// <summary>
        /// 【目的】演出をスキップして即座に表示状態へ遷移する。
        /// </summary>
        void Show();

        /// <summary>
        /// 【目的】標準アニメーションで非表示にする。
        /// </summary>
        UniTask HideAsync();

        /// <summary>
        /// 【目的】タイトル画面からローディング画面への遷移演出を再生する。
        /// </summary>
        UniTask TitleToLoadingAsync();

        /// <summary>
        /// 【目的】ローディング完了後にインゲームへ遷移する演出を再生する。
        /// </summary>
        UniTask LoadingToInGameAsync();
    }
    
    /// <summary>
    /// ローディング画面のプレゼンター（シングルトン）。
    /// - 外部コードからの呼び出し窓口
    /// - View の表示/非表示/遷移アニメーションを操作
    /// - 表示中は UI の操作を無効化し、演出が終わると再度制御を戻す
    /// </summary>
    public class LoadingScreenPresenter: ILoadingScreen
    {
        // View を Presenter 側で握り、各アニメーション呼び出し時に都度探索しないようキャッシュする。
        private ILoadingScreenView _view;
        // 複数の非同期処理を束ねてキャンセルするために、Presenter 内で共有の CancellationTokenSource を保持する。
        private CancellationTokenSource _cts;
        
        /// <summary>
        /// 初期化処理。
        /// - View を初期化し、アニメーションコンポーネントを準備
        /// <para>
        /// 【理由】外部から注入された View を保持し、Presenter 側で制御できるようにするため。
        /// </para>
        /// </summary>
        public void Attach(LoadingScreenPresenterContext context)
        {
            _view = context.View;
            _cts = new CancellationTokenSource();
        }
        
        /// <summary>
        /// ローディング画面をアニメーションで表示。
        /// - 表示中は UI 操作を不可にする
        /// </summary>
        public async UniTask ShowAsync()
        {
            _view.SetInteractable(true);
            await _view.ShowAsync(_cts.Token);
        }
        
        /// <summary>
        /// 任意のワールド座標を中心にして表示（クリック位置などを中心に収束演出）。
        /// </summary>
        public async UniTask ShowAsync(Vector3 centerCirclePosition)
        {
            _view.SetInteractable(true);
            await _view.ShowAsync(centerCirclePosition, _cts.Token);
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
            await _view.HideAsync(_cts.Token);
            _view.SetInteractable(false);
        }
        
        /// <summary>
        /// タイトル → ローディング 遷移アニメーション。
        /// </summary>
        public async UniTask TitleToLoadingAsync()
        {
            _view.SetInteractable(true);
            await _view.TitleToLoadingAsync(_cts.Token);
        }
        
        /// <summary>
        /// ローディング → インゲーム 遷移アニメーション。
        /// - 遷移後は UI 操作を不可にする
        /// </summary>
        public async UniTask LoadingToInGameAsync()
        {
            await _view.LoadingToInGameAsync(_cts.Token);
            _view.SetInteractable(false);
        }

        /// <summary>
        /// 【目的】CancellationTokenSource を破棄し、ライフサイクル終了時のリークを防ぐ。
        /// </summary>
        public void Detach()
        {
            _cts.Cancel();
            _cts.Dispose();
        }
    }
    
    /// <summary>
    /// ローディング画面 Presenter へ渡すコンテキスト。
    /// <para>
    /// 【理由】DI から渡す依存が増えた際も型安全に拡張できるよう、構造体でまとめておく。
    /// </para>
    /// </summary>
    public class LoadingScreenPresenterContext
    {
        /// <summary>
        /// 【目的】Presenter が操作対象とする View 参照を保持する。
        /// </summary>
        public ILoadingScreenView View;
    }
}
