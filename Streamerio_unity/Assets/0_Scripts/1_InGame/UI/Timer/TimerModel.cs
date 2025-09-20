using System.Threading;
using R3;
using UnityEngine;

namespace InGame.UI.Timer
{
    /// <summary>
    /// タイマー
    /// </summary>
    public class TimerModel
    {
        private const float _countInterval = 1f;
        
        private ReactiveProperty<float> _valueProp;
        public ReadOnlyReactiveProperty<float> ValueProp => _valueProp;
        
        public TimerModel(float initialTime)
        {
            _valueProp = new (initialTime);
        }

        /// <summary>
        /// カウントダウンスタート
        /// </summary>
        /// <param name="ct">発行された時に、止まる</param>
        public void StartCountdownTimer(CancellationToken ct)
        {
            float time = 0f;
            
            Observable.EveryUpdate()
                .Where(_ => _valueProp.Value > 0)
                .Subscribe(_ =>
                {
                    time += Time.deltaTime;

                    if (time > _countInterval)
                    {
                        _valueProp.Value -= _countInterval;
                        time -= _countInterval;
                    }
                }).RegisterTo(ct);
        }
    }
}