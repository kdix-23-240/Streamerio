using System;
using Alchemy.Inspector;
using R3;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Common.UI.Display.Background
{
    /// <summary>
    /// UIの背景
    /// </summary>
    public class DisplayBackground: UIBehaviourBase, IPointerClickHandler
    {
        [SerializeField, ReadOnly]
        private GameObject _gameObject;
        
        private Subject<Unit> _onClickEvent;
        /// <summary>
        /// 背景をクリックした時のイベント
        /// </summary>
        public Observable<Unit> OnClickAsObservable => _onClickEvent;

#if UNITY_EDITOR
        private void OnValidate()
        {
            _gameObject = gameObject;
        }
#endif
        
        /// <summary>
        /// 初期化
        /// </summary>
        public void Initialize()
        {
            _onClickEvent = new Subject<Unit>();
        }
        
        /// <summary>
        /// 表示
        /// </summary>
        public void Show()
        {
            _gameObject.SetActive(true);
        }
        
        /// <summary>
        /// 非表示
        /// </summary>
        public void Hide()
        {
            _gameObject.SetActive(false);
        }
        
        public void OnPointerClick(PointerEventData eventData)
        {
            _onClickEvent?.OnNext(Unit.Default);
        }
    }
}