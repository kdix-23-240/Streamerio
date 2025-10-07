using Cysharp.Threading.Tasks;
using DG.Tweening;
using System.Threading;
using UnityEngine;

namespace Common.UI.Animation
{
    /// <summary>
    /// UI アニメーションの基底インターフェース。
    /// - 各種 UI アニメーションコンポーネントが実装する
    /// - 非同期でアニメーションを実行し、キャンセル対応も可能
    /// </summary>
    public interface IUIAnimationComponent
    {
        /// <summary>
        /// アニメーションを再生する。
        /// - UniTask を返すので await 可能
        /// - CancellationToken による中断に対応
        /// </summary>
        UniTask PlayAsync(CancellationToken ct);
    }
    
    /// <summary>
    /// DOTween Sequence を使ったアニメーションの基底クラス。
    /// - コンストラクタで Sequence を生成し、AutoKill を無効化して使い回し可能に設定
    /// - 派生クラスはコンストラクタ内で Sequence にアニメーションを組み立てる
    /// - PlayAsync で Restart して最初から再生し、完了まで待機できる
    /// </summary>
    public abstract class SequenceAnimationComponentBase : IUIAnimationComponent
    {
        /// <summary>
        /// 管理対象の DOTween Sequence。
        /// - SetAutoKill(false) により一度完了しても破棄されず、再利用可能
        /// - Pause() により生成直後は停止状態
        /// </summary>
        protected Sequence Sequence;
        
        protected SequenceAnimationComponentBase()
        {
            Sequence = DOTween.Sequence()
                .SetAutoKill(false) // 完了しても破棄されず再利用可能
                .Pause();           // 初期状態は停止
        }
        
        /// <summary>
        /// アニメーションを再生（async 対応）。
        /// - Sequence.Restart() により最初から再生
        /// - UniTask.WaitUntil で再生完了まで待機
        /// - キャンセルが来たら途中で停止可能
        /// </summary>
        public async UniTask PlayAsync(CancellationToken ct)
        {
            Sequence.Restart();

            // IsActive=false または IsPlaying=false になったら完了
            await UniTask.WaitUntil(
                () => !Sequence.IsActive() || !Sequence.IsPlaying(),
                cancellationToken: ct
            );
        }
    }

    /// <summary>
    /// UI アニメーションの共通パラメータ。
    /// - 再生時間
    /// - イージング
    /// などの基本的な設定を保持
    /// </summary>
    public class UIAnimationComponentParamSO: ScriptableObject
    {
        [Header("アニメーションの時間(秒)")]
        [SerializeField, Min(0.01f)]
        public float DurationSec = 0.5f;

        [Header("イージング種類")]
        [SerializeField]
        public Ease Ease = Ease.Linear;
    }
}