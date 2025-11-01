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
    /// クリックイベントの仲介クラス。
    /// - 任意の Observable を購読し、クリックイベントとして配信
    /// - 一定間隔の連打防止 (ThrottleFirst)
    /// - クリック時に SE を再生
    /// </summary>
    public class ClickEventBinder : UIBehaviour, IDisposable
    {
        [SerializeField, LabelText("SE")]
        private SEType _seType = SEType.SNESRPG01;

        [SerializeField, LabelText("ボタンのクリック間隔(秒)")]
        private float _clickIntervalSec = 0.5f;
        
        private Subject<Unit> _clickEvent;
        /// <summary>
        /// 外部から購読可能なクリックイベント
        /// </summary>
        public Observable<Unit> ClickEvent => _clickEvent;
        
        private CancellationTokenSource _cts;

        /// <summary>
        /// 初期化処理。
        /// - 内部イベントストリームを生成
        /// </summary>
        public void Initialize()
        {
            _clickEvent = new Subject<Unit>();
        }
        
        /// <summary>
        /// 指定されたクリック系 Observable を購読し、ClickEvent を発火させる。
        /// - 連打防止のため一定時間内の複数クリックを間引く
        /// - SE を再生
        /// </summary>
        /// <param name="clickObservable">UI のクリックイベントなどの Observable</param>
        public void BindClickEvent<T>(Observable<T> clickObservable)
        {
            _cts = CancellationTokenSource.CreateLinkedTokenSource(destroyCancellationToken);
            
            clickObservable
                .ThrottleFirst(TimeSpan.FromSeconds(_clickIntervalSec)) // 連打防止
                .Subscribe(_ =>
                {
                    AudioManager.Instance.PlayAsync(_seType, destroyCancellationToken).Forget();
                    _clickEvent.OnNext(Unit.Default);
                })
                .RegisterTo(destroyCancellationToken);
        }

        /// <summary>
        /// リソース破棄処理。
        /// - 購読解除
        /// - CancellationTokenSource を破棄
        /// </summary>
        public void Dispose()
        {
            _cts?.Cancel();
            _cts?.Dispose();
            _cts = null;
        }
    }
}