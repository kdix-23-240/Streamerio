using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Common.UI.Display
{
    /// <summary>
    /// Display を提供するサービスのインターフェース。
    /// - 型ごとに Display を取得できる
    /// - 生成済みならキャッシュから再利用される
    /// </summary>
    public interface IDisplayService
    {
        /// <summary>
        /// 指定された型の Display を取得する。
        /// - 既に生成済みならキャッシュから返す
        /// - 未生成なら生成・初期化して返す
        /// </summary>
        /// <typeparam name="TDisplay">取得対象の Display 型</typeparam>
        /// <returns>生成またはキャッシュ済みの Display</returns>
        TDisplay GetDisplay<TDisplay>() 
            where TDisplay : UIBehaviour, IDisplay;
    }
    
    /// <summary>
    /// Display を生成・初期化・キャッシュするための基底クラス。
    /// - Repository から対象のプレハブを検索
    /// - Transform 配下にインスタンス化
    /// - 初期化処理を呼び出し
    /// - 型ごとにキャッシュして再利用
    /// </summary>
    public abstract class DisplayServiceBase: IDisplayService
    {
        private readonly IDisplayRepository _repository;
        private readonly Transform _parent;
        
        /// <summary>
        /// 型ごとの Display キャッシュ
        /// </summary>
        private readonly Dictionary<Type, IDisplay> _displayCache;
        
        protected DisplayServiceBase(IDisplayRepository repository, Transform parent)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _parent = parent ?? throw new ArgumentNullException(nameof(parent));
            _displayCache = new();
        }
        
        /// <summary>
        /// 指定された型の Display を取得する。
        /// - 既に生成済みならキャッシュから返す
        /// - 未生成なら Repository から取得して生成・初期化して返す
        /// </summary>
        public TDisplay GetDisplay<TDisplay>()
            where TDisplay : UIBehaviour, IDisplay
        {
            if (TryGetCachedDisplay(out TDisplay existDisplay))
            {
                return existDisplay;
            }
            
            var prefab = _repository.FindDisplay<TDisplay>();
            var instance = GameObject.Instantiate(prefab, _parent);
            var display = InitializeDisplay(instance);
            _displayCache.Add(typeof(TDisplay), display);
            return display;
        }
        
        /// <summary>
        /// 既に生成済みの Display をキャッシュから取得する。
        /// </summary>
        protected bool TryGetCachedDisplay<TDisplay>(out TDisplay display)
            where TDisplay : UIBehaviour, IDisplay
        {
            if (_displayCache.TryGetValue(typeof(TDisplay), out var existDisplay))
            {
                display = existDisplay as TDisplay;
                return true;
            }
            
            display = null;
            return false;
        }
        
        /// <summary>
        /// Display 生成後の初期化処理。
        /// 継承先で個別の初期化を実装する。
        /// </summary>
        protected abstract TDisplay InitializeDisplay<TDisplay>(TDisplay display)
            where TDisplay : UIBehaviour, IDisplay;
    }
}