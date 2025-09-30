using System;
using Alchemy.Inspector;
using DG.Tweening;
using UnityEngine;

namespace Common.UI.Animation
{
    /// <summary>
    /// 複数の UI パーツ(CanvasGroup)をまとめてフェードさせるアニメーションコンポーネント。
    /// - DOTween の Sequence を使って順番にアニメーションを構築
    /// - 各パーツ間にディレイを挿入可能
    /// </summary>
    public class FadePartsAnimationComponent : SequenceAnimationComponentBase
    {
        /// <summary>
        /// コンストラクタ。
        /// - 指定された CanvasGroup 配列とパラメータで Sequence を構築
        /// </summary>
        public FadePartsAnimationComponent(CanvasGroup[] canvasGroups, FadePartsAnimationComponentParam param): base()
        {
            SetSequence(canvasGroups, param);
        }

        /// <summary>
        /// DOTween の Sequence を構築。
        /// - 各 CanvasGroup に対してフェードを追加
        /// - パーツ間にインターバルを挿入して「順番に」アニメーションする
        /// </summary>
        private void SetSequence(CanvasGroup[] canvasGroups, FadePartsAnimationComponentParam param)
        {
            foreach (var canvasGroup in canvasGroups)
            {
                Sequence.Join(canvasGroup
                    .DOFade(param.Alpha, param.DurationSec)
                    .SetEase(param.Ease));
                Sequence.AppendInterval(param.ShowDelaySec);
            }
        }
    }

    /// <summary>
    /// FadePartsAnimationComponent の設定用パラメータ。
    /// - Alpha / Duration / Ease は FadeAnimationComponentParam から継承
    /// - ShowDelaySec を追加してパーツごとの遅延を指定可能
    /// </summary>
    [Serializable]
    public class FadePartsAnimationComponentParam : FadeAnimationComponentParam
    {
        [SerializeField, LabelText("パーツごとの表示ディレイ(秒)"), Min(0.001f)]
        public float ShowDelaySec = 0.05f;
    }
}
