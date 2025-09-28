using System.Threading;
using Alchemy.Inspector;
using Common.UI.Animation;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Common.UI.Part.Group
{
    /// <summary>
    /// 複数の UI パーツ（CanvasGroup）をまとめて管理するコンポーネント。
    /// - 表示/非表示をフェードアニメーションで制御
    /// - 各パーツに対して遅延をかけながら順番に表示することも可能
    /// - 即時表示/非表示もサポート
    /// </summary>
    public class CommonUIPartsGroup : UIBehaviour
    {
        [SerializeField, LabelText("アニメーションさせるオブジェクト群")]
        private CanvasGroup[] _uiParts;
        
        [Header("アニメーション")]
        [SerializeField, LabelText("表示アニメーション (フェード + 遅延)")]
        private FadePartsAnimationComponentParam _showFadeAnimationParam = new ()
        {
            Alpha = 1f,
            DurationSec = 0.15f,
            Ease = Ease.InSine,
            ShowDelaySec = 0.1f
        };
        
        [SerializeField, LabelText("非表示アニメーション (フェード + 遅延)")]
        private FadePartsAnimationComponentParam _hideFadeAnimationParam = new ()
        {
            Alpha = 0f,
            DurationSec = 0.1f,
            Ease = Ease.OutSine,
            ShowDelaySec = 0.05f
        };  
        
        private FadePartsAnimationComponent _showAnimations;
        private FadePartsAnimationComponent _hideAnimation;

        /// <summary>
        /// 初期化処理。
        /// - 各 UI パーツ用のフェードアニメーションを生成
        /// </summary>
        public void Initialize()
        {
            _showAnimations = new FadePartsAnimationComponent(_uiParts, _showFadeAnimationParam);
            _hideAnimation  = new FadePartsAnimationComponent(_uiParts, _hideFadeAnimationParam);
        }
        
        /// <summary>
        /// アニメーションで表示。
        /// - 各パーツを順にフェードイン
        /// </summary>
        public async UniTask ShowAsync(CancellationToken ct)
        {
            await _showAnimations.PlayAsync(ct);
        }

        /// <summary>
        /// 即時表示。
        /// - すべてのパーツの透明度を一括で最終値に設定
        /// </summary>
        public void Show()
        {
            SetAlphaParts(_showFadeAnimationParam.Alpha);
        }
        
        /// <summary>
        /// アニメーションで非表示。
        /// - 各パーツを順にフェードアウト
        /// </summary>
        public async UniTask HideAsync(CancellationToken ct)
        {
            await _hideAnimation.PlayAsync(ct);
        }
        
        /// <summary>
        /// 即時非表示。
        /// - すべてのパーツの透明度を一括で最終値に設定
        /// </summary>
        public void Hide()
        {
            SetAlphaParts(_hideFadeAnimationParam.Alpha);
        }
        
        /// <summary>
        /// 全 UI パーツの透明度を一括変更。
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