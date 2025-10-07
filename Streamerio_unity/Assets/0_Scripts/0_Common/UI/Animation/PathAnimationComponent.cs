using System;
using System.Threading;
using Alchemy.Inspector;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

namespace Common.UI.Animation
{
    /// <summary>
    /// 複数の座標を通るパス移動アニメーションコンポーネント。
    /// - DOTween の DOLocalPath を利用
    /// - 指定した Path の座標を順に辿る
    /// - PathType によって直線 / 曲線補間を切り替え可能
    /// - 非同期で再生し、CancellationToken で中断も可能
    /// </summary>
    public class PathAnimationComponent : IUIAnimationComponent
    {
        private readonly RectTransform _rectTransform;
        private readonly PathAnimationComponentParam _param;

        public PathAnimationComponent(RectTransform rectTransform, PathAnimationComponentParam param)
        {
            _rectTransform = rectTransform;
            _param = param;
        }

        /// <summary>
        /// パスに沿って移動アニメーションを再生する。
        /// - Path: 通過する座標の配列
        /// - Duration / Ease / PathType はパラメータで制御
        /// </summary>
        public async UniTask PlayAsync(CancellationToken ct)
        {
            await _rectTransform
                .DOLocalPath(_param.Path, _param.DurationSec, _param.PathType)
                .SetEase(_param.Ease)
                .ToUniTask(cancellationToken: ct);
        }  
    }

    /// <summary>
    /// パス移動アニメーションの設定パラメータ。
    /// - Path: オブジェクトが通る座標リスト（ローカル座標系）
    /// - PathType: 補間方法（Linear / CatmullRom など）
    /// - DurationSec / Ease: アニメーション速度やイージング（基底から継承）
    /// </summary>
    [Serializable]
    public class PathAnimationComponentParam : UIAnimationComponentParam
    {
        [SerializeField, LabelText("オブジェクトが通る点 (先頭から順に)")]
        public Vector3[] Path;

        [SerializeField, LabelText("パスの種類 (直線/曲線)")]
        public PathType PathType = PathType.CatmullRom;
    }
}