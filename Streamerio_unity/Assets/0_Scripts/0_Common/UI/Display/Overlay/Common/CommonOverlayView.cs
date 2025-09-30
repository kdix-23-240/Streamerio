using Alchemy.Inspector;
using Common.UI.Animation;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using System.Threading;
using Common.UI.Display.Background;
using Common.UI.Part.Group;
using UnityEngine;

namespace Common.UI.Display.Overlay
{
    /// <summary>
    /// Overlay 系 UI の見た目をフェードアニメーションで制御する基底クラス。
    /// - 複数のパーツを順番にフェード表示
    /// - 非表示は CanvasGroup をまとめてフェード
    /// - 即時表示/非表示も可能
    /// </summary>
    [RequireComponent(typeof(CommonUIPartGroup))]
    public class CommonOverlayView : DisplayViewBase
    {
        [SerializeField, ReadOnly]
        private DisplayBackgroundPresenter _background;
        public DisplayBackgroundPresenter Background => _background;
        
        [SerializeField, ReadOnly]
        private CommonUIPartGroup _partGroup;
        
        [SerializeField]
        private float _showAlpha = 1f;
        
        [Header("アニメーション")]
        [SerializeField, LabelText("非表示アニメーション")]
        private FadeAnimationComponentParam _hideFadeAnimationParam = new ()
        {
            Alpha = 0f,
            DurationSec = 0.5f,
            Ease = Ease.OutSine,
        };
        
        private FadeAnimationComponent _hideAnimation;

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();
            _partGroup ??= GetComponent<CommonUIPartGroup>();
            _background ??= GetComponentInChildren<DisplayBackgroundPresenter>();
        }
#endif
        
        /// <summary>
        /// 初期化処理。
        /// - 各パーツに対応する表示用フェードアニメーションを構築
        /// - 全体の非表示用アニメーションを構築
        /// </summary>
        public override void Initialize()
        {
            base.Initialize();
            
            _partGroup.Initialize();
            _background.Initialize();
            
            _hideAnimation = new (CanvasGroup, _hideFadeAnimationParam);
        }
        
        /// <summary>
        /// アニメーションで表示。
        /// - CanvasGroup を表示状態に設定
        /// - UI パーツを順番にフェードイン
        /// </summary>
        public override async UniTask ShowAsync(CancellationToken ct)
        {
            CanvasGroup.alpha = _showAlpha;

            await _background.ShowAsync(ct);
            
            await _partGroup.ShowAsync(ct);
        }
        
        /// <summary>
        /// 即時表示。
        /// - CanvasGroup を表示状態に設定
        /// - 全 UI パーツの透明度を一括変更
        /// </summary>
        public override void Show()
        {
            CanvasGroup.alpha = _showAlpha;
            
            _background.Show();
            
            _partGroup.Show();
        }
        
        /// <summary>
        /// アニメーションで非表示。
        /// - 全体 CanvasGroup をフェードアウト
        /// - パーツの透明度も最終値に揃える
        /// </summary>
        public override async UniTask HideAsync(CancellationToken ct)
        {
            await _hideAnimation.PlayAsync(ct);
            
            _background.Hide();
            _partGroup.Hide();
        }

        /// <summary>
        /// 即時非表示。
        /// - CanvasGroup を非表示状態に設定
        /// - 全パーツの透明度を一括変更
        /// </summary>
        public override void Hide()
        {
            CanvasGroup.alpha = _hideFadeAnimationParam.Alpha;
            
            _background.Hide();
            _partGroup.Hide();
        }
    }
}