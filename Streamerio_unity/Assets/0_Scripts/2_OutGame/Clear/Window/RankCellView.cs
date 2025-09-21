using System.Threading;
using Common.UI;
using Common.UI.Animation;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using TMPro;
using UnityEngine;

namespace OutGame.UI.Window
{
    public class RankCellView: UIBehaviourBase
    {
        [SerializeField]
        private TMP_Text _nameText;
        [SerializeField]
        private FadeAnimationComponentParam _showAnimParam = new()
        {
            Alpha = 1f,
            Duration = 0.1f,
            Ease = DG.Tweening.Ease.InSine,
        };
        
        [SerializeField]
        private float _showTextDuration = 1f;
        
        private FadeAnimationComponent _showAnim;

        public override void Initialize()
        {
            _showAnim = new FadeAnimationComponent(CanvasGroup, _showAnimParam);
            CanvasGroup.alpha = 0;
            _nameText.text = string.Empty;
        }
        
        public async UniTask ShowAsync(string name, CancellationToken ct)
        {
            await _showAnim.PlayAsync(ct);
            await _nameText
                .DOText(name, _showTextDuration)
                .SetEase(Ease.Linear)
                .ToUniTask(cancellationToken: ct);
        }
    }
}