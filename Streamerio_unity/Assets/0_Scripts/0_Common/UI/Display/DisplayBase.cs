using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;

namespace Common.UI.Display
{
    /// <summary>
    /// UI 表示制御の共通インターフェース。
    /// - 表示/非表示の状態管理
    /// - 初期化
    /// - アニメーション付き表示/非表示
    /// </summary>
    public interface IDisplay
    {
        /// <summary>
        /// 現在 UI が表示されているかどうか
        /// </summary>
        bool IsShow { get; }
        
        /// <summary>
        /// 初期化処理
        /// </summary>
        void Initialize();
        
        /// <summary>
        /// アニメーション付きで表示する
        /// </summary>
        /// <param name="ct">キャンセル用トークン</param>
        UniTask ShowAsync(CancellationToken ct);
        
        /// <summary>
        /// 即座に表示する
        /// </summary>
        void Show();
        
        /// <summary>
        /// アニメーション付きで非表示にする
        /// </summary>
        /// <param name="ct">キャンセル用トークン</param>
        UniTask HideAsync(CancellationToken ct);
        
        /// <summary>
        /// 即座に非表示にする
        /// </summary>
        void Hide();
    }

    /// <summary>
    /// Display の制御を行う Presenter の基底クラス。
    /// - View を参照して実際の見た目を制御
    /// - 表示状態を管理
    /// - イベントやデータバインディングを設定
    /// </summary>
    /// <typeparam name="TView">対応する View の型</typeparam>
    public abstract class DisplayPresenterBase<TView> : UIBehaviour, IDisplay
        where TView : DisplayViewBase
    {
        protected bool _isShow;
        public bool IsShow => _isShow;
        
        [SerializeField, Alchemy.Inspector.ReadOnly]
        protected TView CommonView;

#if UNITY_EDITOR
        /// <summary>
        /// Inspector 上で View 参照を自動補完
        /// </summary>
        protected override void OnValidate()
        {
            base.OnValidate();
            CommonView ??= GetComponent<TView>();
        }
#endif
        
        /// <summary>
        /// 初期化処理。
        /// - 状態リセット
        /// - View 初期化
        /// - イベント/バインディング設定
        /// </summary>
        public virtual void Initialize()
        {
            _isShow = false;
            
            CommonView.Initialize();
            
            SetEvent();
            Bind();
        }

        /// <summary>
        /// UI のイベント設定（クリックや入力イベントの登録など）
        /// </summary>
        protected virtual void SetEvent() { }

        /// <summary>
        /// データバインディングや購読の設定
        /// </summary>
        protected virtual void Bind() { }
        
        public virtual async UniTask ShowAsync(CancellationToken ct)
        {
            _isShow = true;
            await CommonView.ShowAsync(ct);
            CommonView.SetInteractable(true);
        }

        public virtual void Show()
        {
            _isShow = true;
            CommonView.Show();
            CommonView.SetInteractable(true);
        }

        public virtual async UniTask HideAsync(CancellationToken ct)
        {
            CommonView.SetInteractable(false);
            _isShow = false;
            await CommonView.HideAsync(ct);
        }

        public virtual void Hide()
        {
            CommonView.SetInteractable(false);
            _isShow = false;
            CommonView.Hide();
        }
    }

    /// <summary>
    /// UI の見た目を担う View の基底クラス。
    /// - アニメーションや表示/非表示の実装を担当
    /// - Presenter から制御される
    /// </summary>
    public abstract class DisplayViewBase : UIBehaviourBase
    {
        /// <summary>
        /// アニメーション付きで表示
        /// </summary>
        public abstract UniTask ShowAsync(CancellationToken ct);

        /// <summary>
        /// 即座に表示
        /// </summary>
        public abstract void Show();
        
        /// <summary>
        /// アニメーション付きで非表示
        /// </summary>
        public abstract UniTask HideAsync(CancellationToken ct);

        /// <summary>
        /// 即座に非表示
        /// </summary>
        public abstract void Hide();
    }
}