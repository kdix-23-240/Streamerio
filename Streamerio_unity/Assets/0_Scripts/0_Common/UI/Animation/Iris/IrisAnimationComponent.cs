// ============================================================================
// モジュール概要: イリス式トランジション演出（円の開閉）を DOTween で制御するコンポーネント群。
// 外部依存: Cysharp.Threading.Tasks（UniTask）、DG.Tweening、UnityEngine（Material 操作）。
// 使用例: LoadingScreenView が IrisIn/OutAnimationComponent を利用し、シーン遷移の幕開け/幕閉じを表現する。
// ============================================================================

using System.Threading;
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
        /// <summary>
        /// 【目的】閉じ演出に必要なマテリアルとパラメータを基底へ渡す。
        /// 【理由】Material を共有しつつ半径だけ切り替えることで GC を抑えるため。
        /// </summary>
        public IrisInAnimationComponent(Material material, IrisAnimationComponentParamSO param) 
            : base(material, param) { }

        /// <inheritdoc/>
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
        /// <summary>
        /// 【目的】開き演出に必要なマテリアルとパラメータを基底へ渡す。
        /// </summary>
        public IrisOutAnimationComponent(Material material, IrisAnimationComponentParamSO param) 
            : base(material, param) { }

        /// <inheritdoc/>
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
        /// <summary>
        /// 【目的】アニメーションに必要な半径や中心情報を保持し、派生クラスからも調整可能にする。
        /// </summary>
        protected IrisAnimationComponentParamSO Param;
        
        /// <summary>
        /// 【目的】演出ターゲットとなるマテリアルと設定値を保持する。
        /// 【理由】毎回 Material を検索せず、事前に参照を確保しておくことで描画コストを抑える。
        /// </summary>
        protected IrisAnimationComponent(Material material, IrisAnimationComponentParamSO param)
        {
            _material = material;
            Param = param;
        }
        
        /// <summary>
        /// 【目的】派生クラスで演出方向に応じた再生処理を実装させる抽象メソッド。
        /// 【理由】共通処理は基底に残しつつ、開始/終了半径を差し替えられるようにするため。
        /// </summary>
        public abstract UniTask PlayAsync(CancellationToken ct);
        
        /// <summary>
        /// 【目的】イリスアニメーションを共通ロジックで再生し、中心や半径の設定を一箇所に集約する。
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
}
