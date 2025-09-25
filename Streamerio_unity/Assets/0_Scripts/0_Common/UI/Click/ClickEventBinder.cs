using System;
using System.Threading;
using Alchemy.Inspector;
using Common.Audio;
using Cysharp.Threading.Tasks;
using R3;
using UnityEngine;
using UnityEngine.EventSystems;
using IDisposable = System.IDisposable;

namespace Common.UI.Click
{
    /// <summary>
    /// クリック時のイベント焼き付けを行う
    /// </summary>
    public class ClickEventBinder: UIBehaviour, IDisposable
    {
        [SerializeField, LabelText("SE")]
        private SEType _seType = SEType.SNESRPG01;
        [SerializeField, LabelText("ボタンのクリック間隔(秒)")]
        private float _clickIntervalSec = 0.1f;
        
        private Subject<Unit> _clickEvent;
        /// <summary>
        /// クリックした時のイベント
        /// </summary>
        public Observable<Unit> ClickEvent => _clickEvent;
        
        private CancellationTokenSource _cts;

        /// <summary>
        /// 初期化
        /// </summary>
        public void Initialize()
        {
            _clickEvent = new Subject<Unit>();
        }
        
        /// <summary>
        /// クリック時のイベントを登録
        /// </summary>
        public void BindClickEvent<T>(Observable<T> clickObservable)
        {
            _cts = CancellationTokenSource.CreateLinkedTokenSource(destroyCancellationToken);
            
            clickObservable
                .ThrottleFirst(TimeSpan.FromSeconds(_clickIntervalSec))
                .Subscribe(_ =>
                {
                    AudioManager.Instance.PlayAsync(_seType, destroyCancellationToken).Forget();
                    _clickEvent.OnNext(Unit.Default);
                }).RegisterTo(destroyCancellationToken);
        }

        public void Dispose()
        {
            _cts?.Cancel();
            _cts?.Dispose();
            _cts = null;
        }
    }
}