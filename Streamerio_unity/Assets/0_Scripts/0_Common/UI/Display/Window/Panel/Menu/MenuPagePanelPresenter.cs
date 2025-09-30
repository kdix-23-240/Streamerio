using Alchemy.Inspector;
using Common.Audio;
using Common.Scene;
using Common.UI.Loading;
using Cysharp.Threading.Tasks;
using R3;
using UnityEngine;

namespace Common.UI.Display.Window.Panel
{
    /// <summary>
    /// メニューページのプレゼンター。
    /// - MenuPagePanelView と連携し、ボタン操作を購読する
    /// - 「スタート/遊び方/オプション/クレジット」各ボタンのイベントをバインド
    /// - ボタンクリックに応じて、シーン遷移や UI 遷移を制御する
    /// </summary>
    [RequireComponent(typeof(MenuPagePanelView))]
    public class MenuPagePanelPresenter : PagePanelPresenter
    {
        [SerializeField, ReadOnly]
        private MenuPagePanelView _view;

#if UNITY_EDITOR
        /// <summary>
        /// エディタ上で自動的に View 参照を補完
        /// </summary>
        protected override void OnValidate()
        {
            base.OnValidate();
            _view ??= GetComponent<MenuPagePanelView>();
        }
#endif
        
        /// <summary>
        /// 初期化処理。
        /// - View 初期化
        /// - 親クラスの初期化
        /// </summary>
        public override void Initialize()
        {
            _view.Initialize();
            base.Initialize();
        }

        /// <summary>
        /// ボタンイベントのバインド処理。
        /// - 各種ボタンクリックに対応する遷移/処理を登録
        /// </summary>
        protected override void Bind()
        {
            base.Bind();

            // ゲーム開始ボタン
            _view.StartButton.OnClickAsObservable
                .SubscribeAwait(async (_, ct) =>
                {
                    // タイトル演出 → ローディングへ
                    await LoadingScreenPresenter.Instance.TitleToLoadingAsync();
                    
                    // 通信接続開始
                    WebsocketManager.Instance.ConnectWebSocket();
                    
                    // タイトルBGMを停止
                    AudioManager.Instance.StopBGM();
                    
                    // ゲームシーンへ遷移（非同期実行）
                    SceneManager.Instance.LoadSceneAsync(SceneType.GameScene).Forget();
                }).RegisterTo(destroyCancellationToken);
            
            // 遊び方ボタン
            _view.HowToPlayButton.OnClickAsObservable
                .SubscribeAwait(async (_, ct) =>
                {
                    await ChapterManager.Instance
                        .OpenAndWaitCloseAsync<HowToPlayChapterPanelPresenter>(ct);
                }).RegisterTo(destroyCancellationToken);
            
            // オプションボタン
            _view.OptionButton.OnClickAsObservable
                .SubscribeAwait(async (_, ct) =>
                {
                    await ChapterManager.Instance
                        .OpenAndWaitCloseAsync<OptionChapterPanelPresenter>(ct);
                }).RegisterTo(destroyCancellationToken);
            
            // クレジットボタン
            _view.CreditButton.OnClickAsObservable
                .SubscribeAwait(async (_, ct) =>
                {
                    await ChapterManager.Instance
                        .OpenAndWaitCloseAsync<CreditChapterPanelPresenter>(ct);
                }).RegisterTo(destroyCancellationToken);
        }
    }
}