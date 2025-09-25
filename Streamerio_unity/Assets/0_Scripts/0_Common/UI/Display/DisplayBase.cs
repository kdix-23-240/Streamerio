using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Common.UI.Display
{
    public interface IDisplay
    {
        /// <summary>
        /// UIを表示しているか
        /// </summary>
        bool IsShow { get; }
        
        /// <summary>
        /// 初期化
        /// </summary>
        void Initialize();
        
        /// <summary>
        /// アニメーションで表示
        /// </summary>
        /// <param name="ct"></param>
        /// <returns></returns>
        UniTask ShowAsync(CancellationToken ct);
        /// <summary>
        /// 表示
        /// </summary>
        void Show();
        
        /// <summary>
        /// アニメーションで非表示
        /// </summary>
        /// <param name="ct"></param>
        /// <returns></returns>
        UniTask HideAsync(CancellationToken ct);
        /// <summary>
        /// 非表示
        /// </summary>
        void Hide();
    }

    /// <summary>
    /// UIのつなぎ役の基底クラス
    /// </summary>
    /// <typeparam name="TView"></typeparam>
    public abstract class DisplayPresenterBase<TView>: UIBehaviour, IDisplay
        where TView: DisplayViewBase
    {
        protected bool _IsShow;
        public bool IsShow => _IsShow;
        
        [SerializeField, Alchemy.Inspector.ReadOnly]
        protected TView View;

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();
            
            View ??= GetComponent<TView>();
        }
#endif
        
        public virtual void Initialize()
        {
            _IsShow = false;
            
            View.Initialize();
            
            SetEvent();
            Bind();
        }

        /// <summary>
        /// イベントの設定
        /// </summary>
        protected virtual void SetEvent()
        {
            
        }

        /// <summary>
        /// イベントの焼き付け
        /// </summary>
        protected virtual void Bind()
        {
            
        }
        
        public virtual async UniTask ShowAsync(CancellationToken ct)
        {
            _IsShow = true;
            await View.ShowAsync(ct);
            View.SetInteractable(true);
        }

        public virtual void Show()
        {
            _IsShow = true;
            View.Show();
            View.SetInteractable(true);
        }

        public virtual async UniTask HideAsync(CancellationToken ct)
        {
            View.SetInteractable(false);
            _IsShow = false;
            await View.HideAsync(ct);
        }

        public virtual void Hide()
        {
            View.SetInteractable(false);
            _IsShow = false;
            View.Hide();
        }
    }

    /// <summary>
    /// UIの見た目の基底クラス
    /// </summary>
    public abstract class DisplayViewBase : UIBehaviourBase
    {
        /// <summary>
        /// アニメーションで表示
        /// </summary>
        /// <param name="ct"></param>
        /// <returns></returns>
        public abstract UniTask ShowAsync(CancellationToken ct);
        /// <summary>
        /// 表示
        /// </summary>
        /// <returns></returns>
        public abstract void Show();
        
        /// <summary>
        /// アニメーションで非表示
        /// </summary>
        /// <param name="ct"></param>
        /// <returns></returns>
        public abstract UniTask HideAsync(CancellationToken ct);
        /// <summary>
        /// 非表示
        /// </summary>
        public abstract void Hide();
    }
}