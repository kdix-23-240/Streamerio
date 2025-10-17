// モジュール概要:
// TextMeshPro 等の CanvasGroup を点滅させるコンポーネントで、演出開始/停止を明示的に制御する。
// 依存関係: FlashAnimationComponentParamSO で演出パラメータを受け取り、FlashAnimationComponent がアルファ値を操作する。
// 使用例: Attention を引きたいラベルにアタッチし、Presenter から PlayTextAnimation/StopTextAnimation を呼び出す。

using System.Threading;
using Alchemy.Inspector;
using Common.UI.Animation;
using Cysharp.Threading.Tasks;
using UnityEngine;
using VContainer.Unity;

namespace Common.UI.Part.Text
{
    /// <summary>
    /// 【目的】テキストの点滅演出を抽象化し、開始/停止 API を提供する。
    /// 【理由】複数箇所で同じ点滅挙動を使い回せるようにし、アニメーション設定を ScriptableObject で共有するため。
    /// </summary>
    public class FlashText : UIBehaviourBase, IInitializable
    {
        /// <summary>
        /// 【目的】点滅アニメーションの設定を Inspector から受け取る。
        /// 【理由】点滅速度や強度をデザイナーが調整できるようにし、他テキストと演出を揃えるため。
        /// </summary>
        [SerializeField, LabelText("テキストの点滅アニメーション設定")]
        private FlashAnimationComponentParamSO _flashAnimationParam;
        
        /// <summary>
        /// 【目的】点滅アニメーションを実行するコンポーネントをキャッシュする。
        /// 【理由】Play 毎にインスタンスを生成せず、連続起動時の GC を抑えるため。
        /// </summary>
        private FlashAnimationComponent _flashAnimation;
        /// <summary>
        /// 【目的】再生中のアニメーションを停止させるための CancellationTokenSource を保持する。
        /// 【理由】StopTextAnimation 呼び出しで確実に演出を止め、破棄時のリークを避けるため。
        /// </summary>
        private CancellationTokenSource _cts;

        /// <summary>
        /// 【目的】FlashAnimationComponent を生成し、点滅演出の準備を整える。
        /// 【理由】PlayTextAnimation 呼び出し時に即座に再生を開始できるようにするため。
        /// </summary>
        public void Initialize()
        {
            _flashAnimation = new FlashAnimationComponent(CanvasGroup, _flashAnimationParam);
        }
        
        /// <summary>
        /// 【目的】点滅アニメーションを開始し、アルファ値を周期的に変化させる。
        /// 【理由】ユーザーの注意を引きたいシーンで明滅を開始できるようにするため。
        /// </summary>
        public void PlayTextAnimation()
        {
            _cts = new CancellationTokenSource();
            _flashAnimation.PlayAsync(_cts.Token).Forget();
        }
        
        /// <summary>
        /// 【目的】点滅アニメーションを停止し、透明度を既定値へ戻す。
        /// 【理由】点滅停止後に中途半端な明るさが残らないようにし、演出完了を明確にするため。
        /// </summary>
        public void StopTextAnimation()
        {
            _cts?.Cancel();
            _cts?.Dispose();
            
            CanvasGroup.alpha = _flashAnimationParam.MinAlpha;
        }
    }
}
