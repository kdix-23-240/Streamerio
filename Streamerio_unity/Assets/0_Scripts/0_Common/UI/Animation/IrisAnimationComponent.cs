using System;
using System.Threading;
using Alchemy.Inspector;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

namespace Common.UI.Animation
{
    /// <summary>
    /// 円が閉じるアニメーション（外から内へ収束）。
    /// </summary>
    public class IrisInAnimationComponent : IrisAnimationComponent
    {
        public IrisInAnimationComponent(Material material, IrisAnimationComponentParam param) 
            : base(material, param) { }

        public override async UniTask PlayAsync(CancellationToken ct)
        {
            await PlayIrisAsync(Param.MaxRadius, Param.MinRadius, ct);
        }
    }
    
    /// <summary>
    /// 円が開くアニメーション（内から外へ拡散）。
    /// </summary>
    public class IrisOutAnimationComponent : IrisAnimationComponent
    {
        public IrisOutAnimationComponent(Material material, IrisAnimationComponentParam param) 
            : base(material, param) { }

        public override async UniTask PlayAsync(CancellationToken ct)
        {
            await PlayIrisAsync(Param.MinRadius, Param.MaxRadius, ct);
        }
    }
    
    /// <summary>
    /// Iris アニメーションの基底クラス。
    /// - 指定した半径を DOTween で補間しながらシェーダープロパティに反映
    /// - 開閉の方向は派生クラスで制御
    /// </summary>
    public abstract class IrisAnimationComponent : IUIAnimationComponent
    {
        private readonly Material _material;
        protected IrisAnimationComponentParam Param;
        
        protected IrisAnimationComponent(Material material, IrisAnimationComponentParam param)
        {
            _material = material;
            Param = param;
        }
        
        /// <summary>
        /// アニメーションの実行。
        /// </summary>
        public abstract UniTask PlayAsync(CancellationToken ct);
        
        /// <summary>
        /// イリスアニメーションの共通処理。
        /// - 中心座標と開始半径を設定
        /// - Tween で終了半径まで補間
        /// </summary>
        protected async UniTask PlayIrisAsync(float startRadius, float endRadius, CancellationToken ct)
        {
            // 中心位置と初期半径をシェーダに適用
            _material.SetVector(Param.CenterPropertyName, Param.Center);
            _material.SetFloat(Param.RadiusPropertyName, startRadius);
            
            // Tweenで半径を補間しつつシェーダに反映
            await _material
                .DOFloat(endRadius, Param.RadiusPropertyName, Param.DurationSec)
                .SetEase(Param.Ease)
                .ToUniTask(cancellationToken: ct);
        }
    }

    /// <summary>
    /// Irisアニメーションのパラメータ。
    /// - 中心座標
    /// - 半径プロパティ名
    /// - 開閉範囲（Min/Max半径）
    /// </summary>
    [Serializable]
    public class IrisAnimationComponentParam : UIAnimationComponentParam
    {
        [SerializeField, LabelText("円の中心のプロパティ名")]
        public string CenterPropertyName = "_CenterUV";
        
        [SerializeField, LabelText("円の半径のプロパティ名")]
        public string RadiusPropertyName = "_Radius";
        
        [SerializeField, LabelText("円の中心位置 (UV座標)")]
        public Vector2 Center = new(0.5f, 0.5f);
        
        [SerializeField, LabelText("イリスの最小半径")]
        public float MinRadius = 0f;
        
        [SerializeField, LabelText("イリスの最大半径")]
        public float MaxRadius = 1.5f;
    }
}