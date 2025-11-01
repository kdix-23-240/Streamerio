using System.Threading;
using Alchemy.Inspector;
using Common.Scene;
using Common.UI.Click;
using Common.UI.Display.Window;
using Common.UI.Loading;
using Cysharp.Threading.Tasks;
using R3;
using R3.Triggers;
using UnityEngine;

namespace OutGame.UI.Display.Window
{
    /// <summary>
    /// ゲームクリア画面のプレゼンター。
    /// - GameClearWindowView と連携し、表示・非表示や演出を制御
    /// - ユーザーのクリックを契機にタイトルシーンへ遷移
    /// - Show/Hide のライフサイクルにフラッシュ演出を紐付け
    /// </summary>
    [RequireComponent(typeof(GameClearWindowView), typeof(ObservableEventTrigger), typeof(ClickEventBinder))]
    public class GameClearWindowPresenter : WindowPresenterBase
    {
        [SerializeField, ReadOnly]
        private GameClearWindowView _view; // クリア画面の見た目（テキスト・演出）

        [SerializeField, ReadOnly]
        private ObservableEventTrigger _clickTrigger; // R3 の PointerClick イベント検知

        [SerializeField, ReadOnly]
        private ClickEventBinder _clickEventBinder; // ボタンクリック購読用ラッパー

#if UNITY_EDITOR
        /// <summary>
        /// Inspector 上で参照が未設定なら自動補完。
        /// </summary>
        protected override void OnValidate()
        {
            base.OnValidate();
            _view ??= GetComponent<GameClearWindowView>();
            _clickTrigger ??= GetComponent<ObservableEventTrigger>();
            _clickEventBinder ??= GetComponent<ClickEventBinder>();
        }
#endif
        
        /// <summary>
        /// 初期化処理。
        /// - 仮のテキストを View に設定（TODO: 動的データを渡す想定）
        /// - ClickEventBinder を初期化
        /// - 基底クラスの初期化も実行
        /// </summary>
        public override void Initialize()
        {
            _view.Initialize("test1", "test2", "test3");
            _clickEventBinder.Initialize();
            base.Initialize();
        }

        /// <summary>
        /// イベント購読設定。
        /// - 本体クリック：タイトルシーン遷移
        /// - 背景クリック：同じくタイトルシーン遷移
        /// </summary>
        protected override void Bind()
        {
            base.Bind();
            Debug.Log("GameClearWindowPresenter: Bind Click Events");
            
            // ClickEventBinder に R3 のクリックイベントをバインド
            _clickEventBinder.BindClickEvent(_clickTrigger.OnPointerClickAsObservable());

            // 本体クリック
            _clickEventBinder.ClickEvent
                .Subscribe(_ => ClickEventAsync().Forget())
                .RegisterTo(destroyCancellationToken);

            // 背景クリック
            CommonView.Background.OnClickAsObservable
                .Subscribe(_ => ClickEventAsync().Forget())
                .RegisterTo(destroyCancellationToken);
        }

        /// <summary>
        /// クリックイベント共通処理。
        /// - ローディング演出を表示
        /// - タイトルシーンを非同期でロード
        /// </summary>
        private async UniTask ClickEventAsync()
        {
            Debug.Log("GameClearWindowPresenter: Clicked, Load Title Scene");
            await LoadingScreenPresenter.Instance.ShowAsync();
            SceneManager.Instance.LoadSceneAsync(SceneType.Title).Forget();
        }

        /// <summary>
        /// 表示処理（アニメーションあり）。
        /// - ウィンドウを表示 → フラッシュ演出を開始
        /// </summary>
        public override async UniTask ShowAsync(CancellationToken ct)
        {
            await base.ShowAsync(ct);
            _view.StartFlash();
        }

        /// <summary>
        /// 表示処理（即時）。
        /// - ウィンドウを即時表示 → フラッシュ演出を開始
        /// </summary>
        public override void Show()
        {
            base.Show();
            _view.StartFlash();
        }
        
        /// <summary>
        /// 非表示処理（アニメーションあり）。
        /// - フラッシュ演出を停止 → ウィンドウを閉じる
        /// </summary>
        public override async UniTask HideAsync(CancellationToken ct)
        {
            _view.StopFlash();
            await base.HideAsync(ct);
        }
        
        /// <summary>
        /// 非表示処理（即時）。
        /// - フラッシュ演出を停止 → ウィンドウを即時閉じる
        /// </summary>
        public override void Hide()
        {
            _view.StopFlash();
            base.Hide();
        }
    }
}
