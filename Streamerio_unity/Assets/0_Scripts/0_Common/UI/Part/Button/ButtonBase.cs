using Alchemy.Inspector;
using Common.UI.Click;
using R3;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Common.UI.Part.Button
{
    /// <summary>
    /// カスタムボタンの基底クラス。
    /// - UnityEngine.UI.Button を内包
    /// - ClickEventBinder を利用してクリックイベントを購読可能にする
    /// - 押下/離上/ホバーなどのイベントを抽象メソッドで提供
    /// - 状態リセットや操作可能切替の共通処理を実装
    /// </summary>
    [RequireComponent(typeof(UnityEngine.UI.Button), typeof(ClickEventBinder))]
    public abstract class ButtonBase : UIBehaviourBase,
        IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField, ReadOnly]
        private UnityEngine.UI.Button _button;
        
        [SerializeField, ReadOnly]
        private ClickEventBinder _clickEventBinder;
        
        /// <summary>
        /// ボタンクリック時のイベント購読用 Observable
        /// </summary>
        public Observable<Unit> OnClickAsObservable => _clickEventBinder.ClickEvent; 

#if UNITY_EDITOR
        /// <summary>
        /// エディタ上で参照を自動補完
        /// </summary>
        protected override void OnValidate()
        {
            base.OnValidate();

            _button ??= GetComponent<UnityEngine.UI.Button>();
            _clickEventBinder ??= GetComponent<ClickEventBinder>();
        }
#endif

        /// <summary>
        /// 初期化処理。
        /// - ClickEventBinder の初期化
        /// - 購読処理のセットアップ
        /// </summary>
        public override void Initialize()
        {
            _clickEventBinder.Initialize();
            base.Initialize();
            Bind();
        }

        /// <summary>
        /// ボタンイベントの購読設定。
        /// - Unity Button の OnClick を ClickEventBinder にバインド
        /// - クリック後に ResetButtonState を呼び出す
        /// </summary>
        private void Bind()
        {
            _clickEventBinder.BindClickEvent(_button.OnClickAsObservable());
            
            OnClickAsObservable
                .Subscribe(_ =>
                {
                    ResetButtonState();
                })
                .RegisterTo(destroyCancellationToken);
        }

        /// <summary>
        /// ボタン押下時の処理（必須実装）
        /// </summary>
        public abstract void OnPointerDown(PointerEventData eventData);

        /// <summary>
        /// ボタン解放時の処理（必須実装）
        /// </summary>
        public abstract void OnPointerUp(PointerEventData eventData);

        /// <summary>
        /// ホバー開始時の処理（必須実装）
        /// </summary>
        public abstract void OnPointerEnter(PointerEventData eventData);

        /// <summary>
        /// ホバー終了時の処理（必須実装）
        /// </summary>
        public abstract void OnPointerExit(PointerEventData eventData);
        
        /// <summary>
        /// ボタンの見た目をデフォルト状態にリセットする処理（必須実装）
        /// </summary>
        protected abstract void ResetButtonState();
        
        /// <summary>
        /// ボタンの操作可否を切り替える
        /// </summary>
        /// <param name="isInteractable">true: 押下可能, false: 押下不可</param>
        public void SetInteractable(bool isInteractable)
        {
            _button.interactable = isInteractable;
        }
    }
}