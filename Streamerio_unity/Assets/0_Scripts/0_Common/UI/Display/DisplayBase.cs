using Cysharp.Threading.Tasks;
using System.Threading;
using Common.UI.Guard;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Common.UI.Display
{
    public interface IDisplay
    {
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
            ClickGuard.Instance.Guard(true);
            await View.ShowAsync(ct);
            ClickGuard.Instance.Guard(false);
        }

        public virtual void Show()
        {
            View.Show();
            ClickGuard.Instance.Guard(false);
        }

        public virtual async UniTask HideAsync(CancellationToken ct)
        {
            ClickGuard.Instance.Guard(true);
            await View.HideAsync(ct);
            ClickGuard.Instance.Guard(false);
        }

        public virtual void Hide()
        {
            View.Hide();
            ClickGuard.Instance.Guard(false);
        }
    }

    /// <summary>
    /// UIの見た目の基底クラス
    /// </summary>
    public abstract class DisplayViewBase : UIBehaviourBase, IDisplay
    {
        public abstract UniTask ShowAsync(CancellationToken ct);
        public abstract void Show();
        
        public abstract UniTask HideAsync(CancellationToken ct);
        public abstract void Hide();
    }
}