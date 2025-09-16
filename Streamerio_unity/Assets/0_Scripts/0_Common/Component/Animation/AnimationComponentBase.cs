using Cysharp.Threading.Tasks;
using DG.Tweening;
using System.Threading;
using UnityEngine;

namespace Common.Component
{
    public abstract class AnimationComponentBase: UnityEngine.Component
    {
        [SerializeField, Header("アニメーションの時間")]
        private float _duration;
        protected float Duration => _duration;
        [SerializeField, Header("イージング")]
        private Ease _ease;
        protected Ease Ease => _ease;

        private LinkedCancellationToken _linkedCt;
        protected LinkedCancellationToken LinkedLinkedCt => _linkedCt;

        /// <summary>
        /// 初期化(使用前に必ず呼ぶ)
        /// </summary>
        /// <param name="target">アニメーションさせるオブジェクト</param>
        public virtual void Initialize(GameObject target)
        {
            _linkedCt = new LinkedCancellationToken(target.GetCancellationTokenOnDestroy());
        }
    }
}