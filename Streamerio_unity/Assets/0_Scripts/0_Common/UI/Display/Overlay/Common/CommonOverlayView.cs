using Alchemy.Inspector;
using Common.UI.Animation;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using System.Threading;
using Common.UI.Display.Background;
using UnityEngine;

namespace Common.UI.Display.Overlay
{
    /// <summary>
    /// Overlay 系 UI の見た目をフェードアニメーションで制御する基底クラス。
    /// - 複数のパーツを順番にフェード表示
    /// - 非表示は CanvasGroup をまとめてフェード
    /// - 即時表示/非表示も可能
    /// </summary>
    public class CommonOverlayView : DisplayViewBase
    {
        [SerializeField, ReadOnly]
        private DisplayBackgroundPresenter _background;
        public DisplayBackgroundPresenter Background => _background;
        [SerializeField, LabelText("アニメーションさせるオブジェクト")]
        private CanvasGroup[] _uiParts;
        
        [Header("アニメーション")]
        [SerializeField, LabelText("表示アニメーション")]
        private FadeAnimationComponentParam _showFadeAnimationParam = new ()
        {
            Alpha = 1f,
            Duration = 0.1f,
            Ease = Ease.InSine,
        };

        [SerializeField, LabelText("パーツごとの表示ディレイ(秒)")]
        private float _showDelaySec = 0.1f;
        
        [SerializeField, LabelText("非表示アニメーション")]
        private FadeAnimationComponentParam _hideFadeAnimationParam = new ()
        {
            Alpha = 0f,
            Duration = 0.1f,
            Ease = Ease.OutSine,
        };
        
        private FadeAnimationComponent[] _showAnimations;
        private FadeAnimationComponent _hideAnimation;

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();
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
            
            _background.Initialize();
            
            _showAnimations = new FadeAnimationComponent[_uiParts.Length];
            for (int i = 0; i < _uiParts.Length; i++)
            {
                _showAnimations[i] = new FadeAnimationComponent(_uiParts[i], _showFadeAnimationParam);
            }
            
            _hideAnimation = new FadeAnimationComponent(CanvasGroup, _hideFadeAnimationParam);
        }
        
        /// <summary>
        /// アニメーションで表示。
        /// - CanvasGroup を表示状態に設定
        /// - UI パーツを順番にフェードイン
        /// </summary>
        public override async UniTask ShowAsync(CancellationToken ct)
        {
            CanvasGroup.alpha = _showFadeAnimationParam.Alpha;

            await _background.ShowAsync(ct);
            
            foreach (var anim in _showAnimations)
            {
                await anim.PlayAsync(ct);
                await UniTask.WaitForSeconds(_showDelaySec, cancellationToken: ct);   
            }
        }
        
        /// <summary>
        /// 即時表示。
        /// - CanvasGroup を表示状態に設定
        /// - 全 UI パーツの透明度を一括変更
        /// </summary>
        public override void Show()
        {
            CanvasGroup.alpha = _showFadeAnimationParam.Alpha;
            
            _background.Show();
            
            SetAlphaParts(_showFadeAnimationParam.Alpha);
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
            SetAlphaParts(_hideFadeAnimationParam.Alpha);
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
            SetAlphaParts(_hideFadeAnimationParam.Alpha);
        }
        
        /// <summary>
        /// 全 UI パーツの透明度を一括変更
        /// </summary>
        private void SetAlphaParts(float alpha)
        {
            foreach (var part in _uiParts)
            {
                part.alpha = alpha;
            }
        }
    }
}