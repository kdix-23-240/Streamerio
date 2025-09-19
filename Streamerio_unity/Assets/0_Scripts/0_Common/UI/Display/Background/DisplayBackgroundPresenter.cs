using System;
using System.Threading;
using Alchemy.Inspector;
using Cysharp.Threading.Tasks;
using R3;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Common.UI.Display.Background
{
    /// <summary>
    /// UIの背景
    /// </summary>
    [RequireComponent(typeof(DisplayBackgroundView))]
    public class DisplayBackgroundPresenter: DisplayPresenterBase<DisplayBackgroundView>, IPointerClickHandler
    {
        private Subject<Unit> _onClickEvent;
        /// <summary>
        /// 背景をクリックした時のイベント
        /// </summary>
        public Observable<Unit> OnClickAsObservable => _onClickEvent;
        
        /// <summary>
        /// 初期化
        /// </summary>
        public override void Initialize()
        {
            _onClickEvent = new Subject<Unit>();
            base.Initialize();
        }

        public override async UniTask ShowAsync(CancellationToken ct)
        {
            _IsShow = true;
            await View.ShowAsync(ct);
        }
        
        public override void Show()
        {
            _IsShow = true;
            View.Show();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            _onClickEvent?.OnNext(Unit.Default);
        }
        
        /// <summary>
        /// 背景の有効/無効を設定
        /// </summary>
        /// <param name="interactable"></param>
        public void SetInteractable(bool interactable)
        {
            View.SetInteractable(interactable);
        }
    }
}