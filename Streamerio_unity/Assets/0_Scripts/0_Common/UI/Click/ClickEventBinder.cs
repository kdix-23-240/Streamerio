using System;
using System.Threading;
using Common.Audio;
using Cysharp.Threading.Tasks;
using R3;

namespace Common.UI.Click
{
    /// <summary>
    /// クリックイベントを外部へ配信するためのインターフェース。
    /// </summary>
    public interface IClickEventBinder : IDisposable
    {
        /// <summary>
        /// クリックイベントを購読可能な Observable。
        /// </summary>
        Observable<Unit> ClickEvent { get; }

        /// <summary>
        /// 指定された Observable をクリックイベントとしてバインドする。
        /// </summary>
        /// <typeparam name="T">Observable のイベント型</typeparam>
        void BindClickEvent();
    }

    /// <summary>
    /// UI のクリック入力を仲介し、クリックイベントを配信するクラス。
    /// - クリック Observable を購読してイベント化
    /// - Throttle 処理による連打防止
    /// - クリック時に SE 再生
    /// </summary>
    public class ClickEventBinder<T> : IClickEventBinder
    {
        private const float _clickIntervalSec = 0.1f;

        private readonly IAudioFacade _audioFacade;
        private readonly SEType _seType;
        
        private readonly Subject<Unit> _clickEvent;

        private Observable<T> _clickObservable;

        private CancellationTokenSource _cts;

        /// <inheritdoc/>
        public Observable<Unit> ClickEvent => _clickEvent;

        /// <summary>
        /// コンストラクタ。内部イベントストリームを初期化する。
        /// </summary>
        /// <param name="seType">クリック時に再生する SE の種類</param>
        public ClickEventBinder(Observable<T> clickObservable, IAudioFacade audioFacade, SEType seType = SEType.SNESRPG01)
        {
            _clickEvent = new Subject<Unit>();
            _clickObservable = clickObservable;
            
            _audioFacade = audioFacade;
            _seType = seType;
        }

        /// <inheritdoc/>
        public void BindClickEvent()
        {
            _cts = new CancellationTokenSource();

            _clickObservable
                .ThrottleFirst(TimeSpan.FromSeconds(_clickIntervalSec)) // 連打防止
                .Subscribe(_ =>
                {
                    _audioFacade.PlayAsync(_seType, _cts.Token).Forget();
                    _clickEvent.OnNext(Unit.Default);
                })
                .RegisterTo(_cts.Token);
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            _cts?.Cancel();
            _cts?.Dispose();
            _clickEvent?.Dispose();
        }
    }
}
