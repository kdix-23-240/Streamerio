using System.Threading;
using Common;
using Common.Audio;
using Common.UI.Display;
using Common.UI.Display.Window;
using Common.UI.Loading;
using Cysharp.Threading.Tasks;
using OutGame.UI.Display.Screen;
using OutGame.UI.Display.Window;
using UnityEngine;

namespace OutGame.Title
{
    /// <summary>
    /// タイトル画面周りの UI と BGM を統括するコントローラ。
    /// - UI基盤(DisplayBooster)の起動
    /// - タイトルスクリーンの表示/再表示
    /// - タイトル用 BGM の再生/停止
    /// - メインメニュー(ブックウィンドウ)の開閉
    /// - ローディング演出との橋渡し
    /// </summary>
    public class TitleManager : SingletonBase<TitleManager>
    {
        [SerializeField] private TitleScreenPresenter _titleScreen;

        /// <summary>
        /// 起動時の初期化フローを非同期で実行。
        /// 手順:
        /// 1) UI基盤ブースト（Window/Overlay/Dialog）
        /// 2) タイトルスクリーンの初期化と表示
        /// 3) タイトルBGM再生
        /// 4) ローディング非表示
        /// </summary>
        private void Start()
        {
            // 1) UI基盤を最初に起動しておく（後続の表示取得で困らないように）
            DisplayBooster.Instance.Boost();

            // 2) タイトルスクリーン初期化→表示（表示は即時でOK）
            _titleScreen.Initialize();
            _titleScreen.Show();

            // 3) タイトルBGM を再生（キャンセルされても固まらないように Forget）
            AudioManager.Instance.PlayAsync(BGMType.kuraituuro, destroyCancellationToken).Forget();

            // 4) ローディングをアニメーションで閉じる
            //    ここは失敗を UI 側に伝播させる必要が特に無いので Forget で安全に握る
            LoadingScreenPresenter.Instance.HideAsync().Forget();
        }

        /// <summary>
        /// メインメニュー（ブックウィンドウ）を開き、閉じるまで待機。
        /// 閉じた後にタイトルスクリーンを再表示する。
        /// </summary>
        public async UniTask OpenMainMenuAsync(CancellationToken ct)
        {
            // ブックウィンドウを開いて閉じるまで待つ
            await WindowManager.Instance.OpenAndWaitCloseAsync<OutGameBookWindowPresenter>(ct);

            // メニューを閉じたらタイトル画面に戻す
            await _titleScreen.ShowAsync(ct);
        }

        /// <summary>
        /// タイトルから別シーンへ遷移する前の演出。
        /// - ローディングのタイトル→ローディング演出
        /// - BGM 停止
        /// </summary>
        public async UniTask BeginTitleTransitionAsync()
        {
            // タイトルの円収束→ローディングへ
            await LoadingScreenPresenter.Instance.TitleToLoadingAsync();

            // BGM はここで止める（演出後に切ることで音切れの違和感を減らす）
            AudioManager.Instance.StopBGM();
        }
    }
}