using System;
using System.Threading;
using Alchemy.Inspector;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

namespace Common.UI.Animation
{
    /// <summary>
    /// 円が開くアニメーション
    /// </summary>
    public class IrisInAnimationComponent: IrisAnimationComponent
    {
        public IrisInAnimationComponent(Material material, IrisAnimationComponentParam param) : base(material, param)
        {
        }

        public override async UniTask PlayAsync(CancellationToken ct)
        {
            await PlayIrisAsync(Param.MaxRadius, Param.MinRadius, ct);
        }
    }
    
    /// <summary>
    /// 円が閉じるアニメーション
    /// </summary>
    public class IrisOutAnimationComponent: IrisAnimationComponent
    {
        public IrisOutAnimationComponent(Material material, IrisAnimationComponentParam param) : base(material, param)
        {
        }

        public override async UniTask PlayAsync(CancellationToken ct)
        {
            await PlayIrisAsync(Param.MinRadius, Param.MaxRadius, ct);
        }
    }
    
    public abstract class IrisAnimationComponent: IUIAnimationComponent
    {
        private readonly Material _material;
        protected IrisAnimationComponentParam Param;
        
        public IrisAnimationComponent(Material material, IrisAnimationComponentParam param)
        {
            _material = material;
            Param = param;
        }
        
        public abstract UniTask PlayAsync(CancellationToken ct);
        
        /// <summary>
        /// イリスアニメーション再生
        /// </summary>
        /// <param name="startRadius"></param>
        /// <param name="endRadius"></param>
        /// <param name="ct"></param>
        protected async UniTask PlayIrisAsync(float startRadius, float endRadius, CancellationToken ct)
        {
            _material.SetVector(Param.CenterPropertyName, Param.Center);
            
            await DOTween.To(() => startRadius, 
                    x =>
                    {
                        startRadius = x;
                        _material.SetFloat(Param.RadiusPropertyName, startRadius);
                    }, 
                    endRadius, 
                    Param.Duration)
                .SetEase(Param.Ease)
                .ToUniTask(cancellationToken: ct);
        }
    }

    [Serializable]
    public class IrisAnimationComponentParam: UIAnimationComponentParam
    {
        [SerializeField, LabelText("円の中心のプロパティ名")]
        public string CenterPropertyName = "_CenterUV";
        [SerializeField, LabelText("円の半径のプロパティ名")]
        public string RadiusPropertyName = "_Radius";
        
        [SerializeField, LabelText("円の中心位置")]
        public Vector2 Center = new(0.5f, 0.5f);
        
        [SerializeField, LabelText("イリスアウトの最小半径")]
        public float MinRadius = 0f;
        [SerializeField, LabelText("イリスアウトの最大半径")]
        public float MaxRadius = 1.5f;
    }
}