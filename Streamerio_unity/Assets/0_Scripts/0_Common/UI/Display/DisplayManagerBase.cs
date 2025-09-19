using Alchemy.Inspector;
using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Common.UI.Display
{
    /// <summary>
    /// UIの表示/非表示の管理
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class DisplayManagerBase<T, TManager>: SingletonBase<TManager>
        where T: IDisplay
        where TManager: DisplayManagerBase<T, TManager>
    {
        [SerializeField, Header("開閉するUI")]
        protected List<UIBehaviour> DisplayList;
        
        [SerializeField, LabelText("UIの親")]
        protected Transform Parent;
        
        private Dictionary<Type, IDisplay> _exisitingDisplayDictionary;
        
        private Stack<IDisplay> _currentDisplayStack;

        /// <summary>
        /// 初期化
        /// </summary>
        public virtual void Initialize()
        {
            _exisitingDisplayDictionary = new();
            _currentDisplayStack = new ();
        }
        
        /// <summary>
        /// UIをアニメーションで開く(閉じられるまで待機)
        /// </summary>
        /// <typeparam name="TDisplay"></typeparam>
        /// <param name="ct"></param>
        public virtual async UniTask OpenDisplayAsync<TDisplay>(CancellationToken ct)
            where TDisplay: UIBehaviour, IDisplay
        {
            await ClosePreDisplayAsync(ct);
            
            IDisplay display;
            if (!_exisitingDisplayDictionary.TryGetValue(typeof(TDisplay), out display))
            {
                display = CreateDisplay<TDisplay>();
                _exisitingDisplayDictionary.Add(typeof(TDisplay), display);
            }
            
            _currentDisplayStack.Push(display);
            
            await display.ShowAsync(ct);
            await UniTask.WaitWhile(() => display.IsShow, cancellationToken: ct);
        }

        /// <summary>
        /// UIを生成
        /// </summary>
        /// <typeparam name="TDisplay"></typeparam>
        /// <returns></returns>
        protected virtual IDisplay CreateDisplay<TDisplay>()
            where TDisplay: UIBehaviour, IDisplay
        {
            var display = DisplayList
                .FirstOrDefault(x => x != null && x.TryGetComponent<TDisplay>(out _));

            if (display == null)
            {
                return null;
            }
            
            var newDisplay = Instantiate(display.gameObject, Parent).GetComponent<TDisplay>();
            newDisplay.Initialize();
            Debug.Log($"CreateDisplay: {typeof(TDisplay)}");
            
            return newDisplay;
        }
        
        /// <summary>
        /// UIをアニメーションですべて閉じる
        /// </summary>
        /// <param name="ct"></param>        
        public virtual async UniTask CloseAllDisplayAsync(CancellationToken ct)
        {
            while (_currentDisplayStack.Count > 0)
            {
                var preDisplay = _currentDisplayStack.Pop();
                await preDisplay.HideAsync(ct);
            }
        }

        /// <summary>
        /// 直前に開いたUIをアニメーションで閉じる
        /// </summary>
        /// <param name="ct"></param>
        public virtual async UniTask ClosePreDisplayAsync(CancellationToken ct)
        {
            if (_currentDisplayStack.Count == 0)
            {
                return;
            }
            
            var preDisplay = _currentDisplayStack.Pop();
            await preDisplay.HideAsync(ct);
        }
    }
}