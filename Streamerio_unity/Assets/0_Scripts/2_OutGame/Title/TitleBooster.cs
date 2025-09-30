using Common.Audio;
using Common.UI.Display;
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
    /// - タイトルスクリーンの表示制御
    /// - タイトル用 BGM の再生管理
    /// - ローディング演出の終了処理
    /// 
    /// 起動時にシーン全体の UI 流れを構築する「入口」クラス。
    /// </summary>
    public class TitleBooster : MonoBehaviour
    {
        [SerializeField]
        private TitleScreenPresenter _titleScreen;

        /// <summary>
        /// 起動時に一度だけ呼ばれる Unity ライフサイクル。
        /// タイトル画面のセットアップ手順をまとめて実行する。
        /// </summary>
        private void Start()
        {
            // 1) UI基盤を起動（Window/Overlay/Dialog マネージャーをまとめて初期化）
            DisplayBooster.Instance.Boost();

            // 2) タイトルスクリーンを初期化 → 表示
            _titleScreen.Initialize();
            _titleScreen.Show();

            // 3) タイトル BGM 再生
            //    - 非同期再生
            //    - キャンセル時に例外を投げないように Forget で握り潰す
            AudioManager.Instance.PlayAsync(BGMType.kuraituuro, destroyCancellationToken).Forget();

            // 4) ローディング画面を非表示化（フェードアウト等の演出付き）
            //    - 非同期完了を待つ必要がないため Forget
            LoadingScreenPresenter.Instance.HideAsync().Forget();
        }
    }
}