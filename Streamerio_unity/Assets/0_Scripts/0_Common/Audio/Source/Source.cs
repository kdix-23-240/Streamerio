using System;
using System.Threading;

using Alchemy.Inspector;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

namespace Common.Audio
{
    /// <summary>
    /// 個別のオーディオ再生を担当するコンポーネント。
    /// - Unity の AudioSource を内包して制御を簡略化
    /// - 再生完了や停止時に Dispose を呼んでプールへ返却
    /// - フェード付き停止やミュート切替にも対応
    /// </summary>
    [RequireComponent(typeof(AudioSource))]
    public class Source : MonoBehaviour, IDisposable
    {
        [SerializeField, ReadOnly]
        private AudioSource _audioSource;

        private Action _onDisable;                // 再生終了時に呼ぶコールバック（プールに返す用）
        private LinkedCancellationToken _lct;    // 外部と連動したキャンセルトークン

#if UNITY_EDITOR
        protected void OnValidate()
        {
            // Editor 上で AudioSource を自動取得
            _audioSource ??= GetComponent<AudioSource>();
        }  
#endif

        /// <summary>
        /// 初期化。
        /// - プールから取り出された時に呼び出される
        /// - 終了時にプールへ返却するためのコールバックを保持
        /// </summary>
        public void Initialize(Action onDisable)
        {
            _onDisable = onDisable;
            _lct = new LinkedCancellationToken(destroyCancellationToken);
        }

        /// <summary>
        /// 指定した AudioClip を再生し、終了まで待機。
        /// </summary>
        public async UniTask PlayAsync(AudioClip clip, CancellationToken ct)
        {
            _audioSource.clip = clip;
            _audioSource.Play();

            // 再生が終了するまで待機（外部キャンセルにも対応）
            await UniTask.WaitUntil(() => !_audioSource.isPlaying, cancellationToken: _lct.GetCancellationToken(ct));

            Dispose();
        }

        /// <summary>
        /// 即座に停止。
        /// </summary>
        public void Stop()
        {
            _audioSource.Stop();
            Dispose();
        }

        /// <summary>
        /// 音量を徐々に下げて停止。
        /// </summary>
        public async UniTask StopAsync(float duration, CancellationToken ct)
        {
            await _audioSource
                .DOFade(0, duration)
                .SetEase(Ease.InOutQuad)
                .ToUniTask(cancellationToken: _lct.GetCancellationToken(ct));

            Stop();
        }

        /// <summary>
        /// ミュート設定を変更。
        /// </summary>
        public void SetMute(bool isMute)
        {
            _audioSource.mute = isMute;
        }

        /// <summary>
        /// 再生終了や停止時に呼ばれる。
        /// - コールバックを実行し、プールへ返却
        /// - 内部リソースを解放
        /// </summary>
        public void Dispose()
        {
            _onDisable?.Invoke();
            _onDisable = null;
            _lct.Dispose();
        }
    }
}