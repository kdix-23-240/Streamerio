using Alchemy.Inspector;
using Common.UI.Animation;
using Cysharp.Threading.Tasks;
using System.Threading;
using Common.UI.Part.Group;
using UnityEngine;
using VContainer;

namespace Common.UI.Display.Overlay
{
    public interface IOverlayView : IDisplayView
    {
    }
    
    /// <summary>
    /// Overlay 系 UI の見た目をフェードアニメーションで制御する基底クラス。
    /// - 複数のパーツを順番にフェード表示
    /// - 非表示は CanvasGroup をまとめてフェード
    /// - 即時表示/非表示も可能
    /// </summary>
    public abstract class OverlayViewBase : DisplayViewBase, IOverlayView
    {
        private ICommonUIPartGroup _partGroup;
        
        [Header("アニメーション")] 
        [SerializeField, LabelText("非表示アニメーション")]
        private FadeAnimationComponentParamSO _hideAnimationParam;
        
        private FadeAnimationComponent _hideAnimation;

        [Inject]
        public virtual void Construct(ICommonUIPartGroup partGroup)
        {
            _partGroup = partGroup;
        }

        public override void Initialize()
        {
            base.Initialize();
            
            _hideAnimation = new (CanvasGroup, _hideAnimationParam);
        }
        
        /// <summary>
        /// アニメーションで表示。
        /// - CanvasGroup を表示状態に設定
        /// - UI パーツを順番にフェードイン
        /// </summary>
        public override async UniTask ShowAsync(CancellationToken ct)
        {
            CanvasGroup.alpha = UIUtil.DEFAULT_SHOW_ALPHA;

            await _partGroup.ShowAsync(ct);
        }
        
        /// <summary>
        /// 即時表示。
        /// - CanvasGroup を表示状態に設定
        /// - 全 UI パーツの透明度を一括変更
        /// </summary>
        public override void Show()
        {
            CanvasGroup.alpha = UIUtil.DEFAULT_SHOW_ALPHA;
            
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
            
            _partGroup.Hide();
        }

        /// <summary>
        /// 即時非表示。
        /// - CanvasGroup を非表示状態に設定
        /// - 全パーツの透明度を一括変更
        /// </summary>
        public override void Hide()
        {
            CanvasGroup.alpha = _hideAnimationParam.Alpha;
            
            _partGroup.Hide();
        }
    }
}