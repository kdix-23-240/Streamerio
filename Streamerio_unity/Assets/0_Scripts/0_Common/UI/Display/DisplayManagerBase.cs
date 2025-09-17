using Alchemy.Inspector;
using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace Common.UI.Display
{
    /// <summary>
    /// UIの表示/非表示の管理
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TManager"></typeparam>
    public abstract class DisplayManagerBase<T, TManager>: SingletonBase<TManager>
        where T: Enum
        where TManager: DisplayManagerBase<T, TManager>
    {
        [SerializeField, LabelText("UIの親")]
        protected Transform Parent;
        
        private Dictionary<T, IDisplay> _exisitingDisplayDictionary;
        
        private Queue<IDisplay> _currentDisplayQueue;

        /// <summary>
        /// 初期化
        /// </summary>
        public void Initialize()
        {
            _exisitingDisplayDictionary = new();
            _currentDisplayQueue = new ();
        }
        
        /// <summary>
        /// UIをアニメーションで開く
        /// </summary>
        /// <param name="type">UIの種類</param>
        /// <param name="ct"></param>
        public async UniTask OpenDisplayAsync(T type, CancellationToken ct)
        {
            await ClosePreDisplayAsync(ct);
            
            IDisplay display;
            if (!_exisitingDisplayDictionary.TryGetValue(type, out display))
            {
                display = CreateDisplay(type);
                _exisitingDisplayDictionary.Add(type, display);
            }
            
            Debug.Log(display);
            await display.ShowAsync(ct);
            _currentDisplayQueue.Enqueue(display);
        }

        /// <summary>
        /// UIを生成
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        protected abstract IDisplay CreateDisplay(T type);
        
        /// <summary>
        /// UIをアニメーションですべて閉じる
        /// </summary>
        /// <param name="ct"></param>        
        public async UniTask CloseAllDisplayAsync(CancellationToken ct)
        {
            while (_currentDisplayQueue.Count > 0)
            {
                var preDisplay = _currentDisplayQueue.Dequeue();
                await preDisplay.HideAsync(ct);
            }
        }

        /// <summary>
        /// 直前に開いたUIをアニメーションで閉じる
        /// </summary>
        /// <param name="ct"></param>
        public async UniTask ClosePreDisplayAsync(CancellationToken ct)
        {
            if (_currentDisplayQueue.Count == 0)
            {
                return;
            }
            
            var preDisplay = _currentDisplayQueue.Dequeue();
            await preDisplay.HideAsync(ct);
        }
    }
}