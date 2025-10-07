using Cysharp.Threading.Tasks;
using System.Threading;
using VContainer.Unity;

namespace Common.UI.Display
{
    /// <summary>
    /// UI 表示制御の共通インターフェース。
    /// <para>
    /// - 表示／非表示状態の管理  
    /// - アニメーション付き／即時表示・非表示  
    /// - Presenter 層での統一的な制御を目的とする
    /// </para>
    /// </summary>
    public interface IDisplay
    {
        /// <summary>
        /// 現在 UI が表示されているかどうか。
        /// </summary>
        bool IsShow { get; }
        
        /// <summary>
        /// アニメーション付きで UI を表示する。
        /// </summary>
        /// <param name="ct">キャンセル用トークン</param>
        UniTask ShowAsync(CancellationToken ct);
        
        /// <summary>
        /// 即座に UI を表示する。
        /// </summary>
        void Show();
        
        /// <summary>
        /// アニメーション付きで UI を非表示にする。
        /// </summary>
        /// <param name="ct">キャンセル用トークン</param>
        UniTask HideAsync(CancellationToken ct);
        
        /// <summary>
        /// 即座に UI を非表示にする。
        /// </summary>
        void Hide();
    }

    /// <summary>
    /// View 側の表示制御インターフェース。
    /// <para>
    /// Presenter から呼ばれ、実際のアニメーションや表示／非表示処理を担当する。
    /// </para>
    /// <remarks>
    /// UI の見た目やトランジションを担うロジックは、Presenter ではなく View 側に集約する。
    /// </remarks>
    /// </summary>
    public interface IDisplayView: ICommonUIBehaviour
    {
        /// <summary>
        /// アニメーション付きで UI を表示する。
        /// <para>
        /// フェードイン・スライドインなど、見た目の演出をここで実装する。
        /// Presenter 側から表示要求があったときに呼び出される。
        /// </para>
        /// </summary>
        /// <param name="ct">アニメーションを中断するためのキャンセルトークン</param>
        UniTask ShowAsync(CancellationToken ct);

        /// <summary>
        /// アニメーションなしで即座に UI を表示する。
        /// <para>
        /// 主に初期化時やアニメーションをスキップしたい場合に使用される。
        /// </para>
        /// </summary>
        void Show();

        /// <summary>
        /// アニメーション付きで UI を非表示にする。
        /// <para>
        /// フェードアウト・スライドアウトなど、見た目の演出をここで実装する。
        /// Presenter 側から非表示要求があったときに呼び出される。
        /// </para>
        /// </summary>
        /// <param name="ct">アニメーションを中断するためのキャンセルトークン</param>
        UniTask HideAsync(CancellationToken ct);

        /// <summary>
        /// アニメーションなしで即座に UI を非表示にする。
        /// <para>
        /// 強制的に非表示状態にしたいときや、初期化・画面切り替えなどで利用される。
        /// </para>
        /// </summary>
        void Hide();
    }
    
    /// <summary>
    /// Display の制御を担う Presenter の基底クラス。
    /// <para>
    /// - View を介して UI の見た目を制御  
    /// - 表示状態を管理  
    /// - イベントやデータバインディングを設定
    /// </para>
    /// </summary>
    /// <typeparam name="TView">対応する View の型</typeparam>
    public abstract class DisplayPresenterBase<TView> : IDisplay, IInitializable
        where TView : IDisplayView
    {
        /// <summary>
        /// 現在の表示状態。
        /// </summary>
        protected bool _isShow;
        public bool IsShow => _isShow;
        
        /// <summary>
        /// 共通のView。
        /// </summary>
        protected TView CommonView;
        
        protected DisplayPresenterBase(TView view)
        {
            CommonView = view;
        }

        /// <summary>
        /// 初期化処理。
        /// <para>
        /// - 表示状態のリセット  
        /// - イベント設定・データバインディング呼び出し
        /// </para>
        /// </summary>
        public virtual void Initialize()
        {
            _isShow = false;
            SetEvent();
            Bind();
        }

        /// <summary>
        /// UI のイベント設定（クリックや入力など）。
        /// Presenter 側で View に対するイベント購読を行う。
        /// </summary>
        protected virtual void SetEvent() { }

        /// <summary>
        /// データバインディングや購読設定。
        /// Presenter 側でモデルやシグナルとの接続を行う。
        /// </summary>
        protected virtual void Bind() { }
        
        /// <inheritdoc />
        public virtual async UniTask ShowAsync(CancellationToken ct)
        {
            _isShow = true;
            await CommonView.ShowAsync(ct);
            CommonView.SetInteractable(true);
        }

        /// <inheritdoc />
        public virtual void Show()
        {
            _isShow = true;
            CommonView.Show();
            CommonView.SetInteractable(true);
        }

        /// <inheritdoc />
        public virtual async UniTask HideAsync(CancellationToken ct)
        {
            CommonView.SetInteractable(false);
            _isShow = false;
            await CommonView.HideAsync(ct);
        }

        /// <inheritdoc />
        public virtual void Hide()
        {
            CommonView.SetInteractable(false);
            _isShow = false;
            CommonView.Hide();
        }
    }

    /// <summary>
    /// UI の見た目（View）を担う基底クラス。
    /// <para>
    /// - 実際のアニメーションや CanvasGroup の制御を実装  
    /// - Presenter から呼ばれて表示／非表示処理を行う
    /// </para>
    /// </summary>
    public abstract class DisplayViewBase : UIBehaviourBase, IDisplayView
    {
        /// <inheritdoc />
        public abstract UniTask ShowAsync(CancellationToken ct);

        /// <inheritdoc />
        public abstract void Show();
        
        /// <inheritdoc />
        public abstract UniTask HideAsync(CancellationToken ct);

        /// <inheritdoc />
        public abstract void Hide();
    }
}
