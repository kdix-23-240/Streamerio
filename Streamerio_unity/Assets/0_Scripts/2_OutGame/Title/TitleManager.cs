using Common;
using Common.Audio;
using Common.Scene;
using Common.UI.Display;
using Common.UI.Display.Window;
using Common.UI.Loading;
using Cysharp.Threading.Tasks;
using OutGame.UI.Display.Screen;
using UnityEngine;

namespace OutGame.Title
{
    /// <summary>
    /// タイトル画面周りの UI と BGM を統括するコントローラ。
    /// 主な責務:
    /// - UI 管理基盤（Window / Overlay / Dialog）の初期化
    /// - タイトルスクリーン（“PRESS START”的な画面）の表示制御
    /// - タイトル用 BGM の再生管理
    /// - ローディング演出の終了処理（タイトル入り時の黒画面解除など）
    /// 
    /// 起動時にシーン全体の UI フローを構築する「入口」クラス。
    /// </summary>
    public class TitleManager : SingletonBase<TitleManager>
    {
        [SerializeField]
        private TitleScreenPresenter _titleScreen;

        /// <summary>
        /// タイトルからゲーム本編へ遷移する最中かどうかのフラグ。
        /// - true の間は Title の再表示（ShowScreen）を抑止して二重遷移を防ぐ
        /// </summary>
        private bool _isCloseAll = false;

        /// <summary>
        /// 起動時に一度だけ呼ばれる Unity ライフサイクル。
        /// タイトル画面のセットアップ手順をまとめて実行する。
        /// </summary>
        private void Start()
        {
            // 1) UI 基盤を起動（Window/Overlay/Dialog マネージャーをまとめて初期化）
            DisplayBooster.Instance.Boost();

            // 2) タイトルスクリーンを初期化 → 表示（同期表示で OK）
            _titleScreen.Initialize();
            _titleScreen.Show();

            // 3) タイトル BGM 再生
            //   - 非同期で再生開始
            //   - 破棄時のキャンセル例外などは握り潰す（Forget）
            AudioManager.Instance.PlayAsync(BGMType.kuraituuro, destroyCancellationToken).Forget();

            // 4) ローディング画面をアニメーションで非表示にする
            //   - 完了を待つ必要はないため Forget
            LoadingScreenPresenter.Instance.HideAsync().Forget();
        }

        /// <summary>
        /// タイトルスクリーンを（再）表示する。
        /// - すでにゲームへ遷移開始している場合は何もしない（多重表示対策）
        /// - 非同期版 Show を呼び出し、失敗は握り潰す（UI 再表示の失敗で進行を止めない）
        /// </summary>
        public void ShowScreen()
        {
            if (_isCloseAll) return; // 本編遷移中はタイトルを再表示しない

            _titleScreen.ShowAsync(destroyCancellationToken).Forget();
        }

        /// <summary>
        /// タイトルからインゲームへ遷移する。
        /// 流れ:
        /// 1) 以降の Title 再表示を抑止（_isCloseAll = true）
        /// 2) 画面上に出ているウィンドウ（あれば）を閉じる
        /// 3) タイトル → ローディングの演出を再生
        /// 4) 通信接続開始（WebSocket）
        /// 5) タイトル BGM 停止
        /// 6) ゲームシーンを非同期でロード開始
        /// </summary>
        public async void LoadInGame()
        {
            _isCloseAll = true;

            // 2) 表示中のウィンドウを閉じる（存在しなければ何も起きない）
            await WindowManager.Instance.CloseTopAsync(destroyCancellationToken);

            // 3) タイトル演出 → ローディングへ
            await LoadingScreenPresenter.Instance.TitleToLoadingAsync();

            // 4) 通信接続開始
            WebsocketManager.Instance.ConnectWebSocket();

            // 5) タイトル BGM を停止
            AudioManager.Instance.StopBGM();

            // 6) ゲームシーンへ遷移（非同期で開始し、完了待ちはしない）
            SceneManager.Instance.LoadSceneAsync(SceneType.GameScene).Forget();
        }
    }
}
