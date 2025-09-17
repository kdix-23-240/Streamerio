using System;
using System.Threading;

using Alchemy.Inspector;

using Cysharp.Threading.Tasks;

using DG.Tweening;

using UnityEngine;

namespace Common.Audio
{
    /// <summary>
    /// オーディオソース
    /// </summary>
    [RequireComponent(typeof(AudioSource))]
    public class Source: MonoBehaviour, IDisposable
    {
        [SerializeField, ReadOnly]
        private AudioSource _audioSource;

        private Action _onDisable;
        private LinkedCancellationToken _lct;


#if UNITY_EDITOR
        protected void OnValidate()
        {
            _audioSource = _audioSource==null ? GetComponent<AudioSource>():_audioSource;
        }  
#endif

        /// <summary>
        /// 初期化
        /// </summary>
        /// <param name="onDisable"> 停止時の処理 </param>
        public void Initialize(Action onDisable)
        {
            _onDisable = onDisable;
            _lct = new LinkedCancellationToken(destroyCancellationToken);
        }

        /// <summary>
        /// 曲を再生して停止するまで待つ
        /// </summary>
        /// <param name="clip"> 再生する曲 </param>
        /// <param name="ct"></param>
        public async UniTask PlayAsync(AudioClip clip, CancellationToken ct)
        {
            _audioSource.clip = clip;
            _audioSource.Play();

            await UniTask.WaitUntil(() => !_audioSource.isPlaying, cancellationToken: _lct.GetCancellationToken(ct));

            Dispose();
        }

        /// <summary>
        /// 曲を停止する
        /// </summary>
        public void Stop()
        {
            _audioSource.Stop();
            Dispose();
        }

        /// <summary>
        /// 曲を徐々に音量を下げて停止
        /// </summary>
        /// <param name="duration"></param>
        /// <param name="ct"></param>
        public async UniTask StopAsync(float duration, CancellationToken ct)
        {
            await _audioSource.DOFade(0, duration)
                .SetEase(Ease.InOutQuad)
                .ToUniTask( cancellationToken: _lct.GetCancellationToken(ct));

            Stop();
        }

        /// <summary>
        /// ミュート状態を設定
        /// </summary>
        /// <param name="isMute"></param>
        public void SetMute(bool isMute)
        {
            _audioSource.mute = isMute;
        }

        public void Dispose()
        {
            _onDisable?.Invoke();
            _onDisable = null;
            _lct.Dispose();
        }
    }
}
