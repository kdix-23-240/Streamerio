using System.Threading;
using Alchemy.Inspector;
using Common;
using R3;
using UnityEngine;
using UnityEngine.EventSystems;

namespace InGame.UI.Timer
{
    [RequireComponent(typeof(TimerView))]
    public class TimerPresenter: UIBehaviour
    {
        [SerializeField, ReadOnly]
        private TimerView _view;

        private TimerModel _model;
        private LinkedCancellationToken _lct;
        
#if UNITY_EDITOR
        protected override void OnValidate()
        {
            _view ??= GetComponent<TimerView>();
        }
#endif

        /// <summary>
        /// 初期化
        /// </summary>
        /// <param name="initialValue">タイマーの初期値</param>
        public void Initialize(float initialValue)
        {
            _model = new(initialValue);
            _view.Initialize(initialValue);

            _lct = new();
            
            Bind();
        }
        
        private void Bind()
        {
            _model.ValueProp
                .DistinctUntilChanged()
                .Subscribe(value =>
                {
                    _view.UpdateTimerView(value);
                }).RegisterTo(destroyCancellationToken);
            
            _model.ValueProp
                .Where(value => value <= 0)
                .Subscribe(_ =>
                {
                    InGameManager.Instance.GameOver();
                }).RegisterTo(destroyCancellationToken);
        }

        /// <summary>
        /// カウントダウンタイマー開始
        /// </summary>
        /// <param name="ct"></param>
        public void StartCountdownTimer(CancellationToken ct)
        {
            _model.StartCountdownTimer(_lct.GetCancellationToken(ct));
        }
    }
}