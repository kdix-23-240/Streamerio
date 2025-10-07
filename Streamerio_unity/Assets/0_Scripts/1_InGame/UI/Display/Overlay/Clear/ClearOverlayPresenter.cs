using System.Threading;
using Common.Scene;
using Common.UI.Display.Overlay;
using Cysharp.Threading.Tasks;
using R3;
using UnityEngine;
using Alchemy.Inspector;

namespace InGame.UI.Display.Overlay
{
    /// <summary>
    /// ゲームクリア時のオーバーレイ Presenter。
    /// - 背景クリックでタイトルシーンへ遷移
    /// - クリック誘導テキストのアニメーションを制御
    /// </summary>
    [RequireComponent(typeof(ClearOverlayView))]
    public class ClearOverlayPresenter : OverlayPresenterBase
    {
        [SerializeField, ReadOnly]
        private ClearOverlayView _view;

#if UNITY_EDITOR
        /// <summary>
        /// エディタ上でコンポーネント参照を自動補完
        /// </summary>
        protected override void OnValidate()
        {
            base.OnValidate();
            _view ??= GetComponent<ClearOverlayView>();
        }
#endif
        
        /// <summary>
        /// 初期化処理。
        /// - View の初期化
        /// - 基底クラスの初期化処理を呼ぶ
        /// </summary>
        public override void Initialize()
        {
            _view.Initialize();
            base.Initialize();
        }
        
        /// <summary>
        /// イベント設定。
        /// - 背景クリックでオーバーレイを閉じ、タイトルシーンへ遷移
        /// </summary>
        protected override void SetEvent()
        {
            base.SetEvent();
            
            OnClickAsObservable
                .SubscribeAwait(async (_, ct) =>
                {
                    await HideAsync(ct);
                    SceneManager.Instance.LoadSceneAsync(SceneType.Title).Forget();
                })
                .RegisterTo(destroyCancellationToken);
        }

        /// <summary>
        /// アニメーション付き表示。
        /// - クリック誘導テキストのアニメーションを開始
        /// </summary>
        public override async UniTask ShowAsync(CancellationToken ct)
        { 
            await base.ShowAsync(ct);
            _view.ClickText.PlayTextAnimation();
        }
        
        /// <summary>
        /// 即時表示。
        /// - クリック誘導テキストのアニメーションを開始
        /// </summary>
        public override void Show()
        {
            base.Show();
            _view.ClickText.PlayTextAnimation();
        }
        
        /// <summary>
        /// アニメーション付き非表示。
        /// - クリック誘導テキストのアニメーションを停止
        /// </summary>
        public override async UniTask HideAsync(CancellationToken ct)
        {
            _view.ClickText.StopTextAnimation();
            await base.HideAsync(ct);
        }
        
        /// <summary>
        /// 即時非表示。
        /// - クリック誘導テキストのアニメーションを停止
        /// </summary>
        public override void Hide()
        {
            _view.ClickText.StopTextAnimation();
            base.Hide();
        }
    }
}