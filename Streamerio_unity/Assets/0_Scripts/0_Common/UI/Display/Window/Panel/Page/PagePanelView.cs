using System.Threading;
using Alchemy.Inspector;
using Common.UI.Animation;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

namespace Common.UI.Display.Window.Panel
{
    /// <summary>
    /// ページの見た目
    /// </summary>
    public class PagePanelView: DisplayViewBase
    {
        [SerializeField, LabelText("全パーツ(表示順)")]
        private CanvasGroup[] _contentParts;
        
        [SerializeField, LabelText("1個のパーツの表示アニメーション")]
        private FadeAnimationComponentParam _showContentAnimParam = new()
        {
            Alpha = 1f,
            Duration = 0.1f,
            Ease = Ease.InSine,
        };
        
        [SerializeField, LabelText("パーツの表示アニメーション再生間隔")]
        private float _contentAnimInterval = 0.05f;

        [SerializeField, LabelText("非表示アニメーション")]
        private FadeAnimationComponentParam _hideAnimParam = new()
        {
            Alpha = 0f,
            Duration = 0.2f,
            Ease = Ease.OutSine,
        };
        
        private FadeAnimationComponent[] _contentShowAnims;
        private FadeAnimationComponent _contentHideAnims;

        public override void Initialize()
        {
            base.Initialize();
            
            int length = _contentParts.Length;
            _contentShowAnims = new FadeAnimationComponent[length];
            
            for(int i = 0; i < length; i++)
            {
                _contentShowAnims[i] = new FadeAnimationComponent(_contentParts[i], _showContentAnimParam);
            }
            
            _contentHideAnims = new FadeAnimationComponent(CanvasGroup, _hideAnimParam);
        }

        public override async UniTask ShowAsync(CancellationToken ct)
        {
            CanvasGroup.alpha = _showContentAnimParam.Alpha;
            
            foreach (var anim in _contentShowAnims)
            {
                anim.PlayAsync(ct).Forget();
                await UniTask.WaitForSeconds(_contentAnimInterval, cancellationToken: ct);
            }
        }
        
        public override void Show()
        {
            SetContentAlpha(_showContentAnimParam.Alpha);
        }

        public override async UniTask HideAsync(CancellationToken ct)
        {
            await _contentHideAnims.PlayAsync(ct);
            SetContentAlpha(_hideAnimParam.Alpha);
        }

        public override void Hide()
        {
            SetContentAlpha(_hideAnimParam.Alpha);
        }
        
        /// <summary>
        /// UIの透明度を全て変える
        /// </summary>
        /// <param name="alpha"></param>
        private void SetContentAlpha(float alpha)
        {
            CanvasGroup.alpha = alpha;
            
            foreach (var part in _contentParts)
            {
                part.alpha = alpha;
            }
        }
    }
}