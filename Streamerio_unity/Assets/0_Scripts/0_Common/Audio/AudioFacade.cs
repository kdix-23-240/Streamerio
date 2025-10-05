using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using VContainer;

namespace Common.Audio
{
    /// <summary>
    /// アプリ全体で利用するオーディオ制御の統一インターフェース。  
    /// <para>
    /// - BGM / SE の再生・停止  
    /// - 音量の取得・変更  
    /// - ミュートの取得・切り替え  
    /// など、音に関する操作を一括で提供します。
    /// </para>
    /// </summary>
    public interface IAudioFacade
    {
        /// <summary>
        /// 現在の音量状態を取得します。
        /// </summary>
        IReadOnlyDictionary<SoundType, Volume> VolumeDict { get; }

        /// <summary>
        /// 現在のミュート状態を取得します。
        /// </summary>
        IReadOnlyDictionary<SoundType, bool> MuteDict { get; }

        /// <summary>
        /// 指定したサウンドタイプの音量を変更します。
        /// </summary>
        /// <param name="soundType">対象のサウンドタイプ。</param>
        /// <param name="volume">設定する音量。</param>
        void ChangeVolume(SoundType soundType, Volume volume);

        /// <summary>
        /// 指定したサウンドタイプのミュート状態を切り替えます。
        /// </summary>
        /// <param name="soundType">対象のサウンドタイプ。</param>
        void ToggleMute(SoundType soundType);

        /// <summary>
        /// 指定した BGM を再生します。
        /// </summary>
        /// <param name="bgm">再生する BGM の種類。</param>
        /// <param name="ct">キャンセルトークン（任意）。</param>
        UniTask PlayAsync(BGMType bgm, CancellationToken ct = default);

        /// <summary>
        /// 指定した SE を再生します。
        /// </summary>
        /// <param name="se">再生する SE の種類。</param>
        /// <param name="ct">キャンセルトークン（任意）。</param>
        UniTask PlayAsync(SEType se, CancellationToken ct = default);

        /// <summary>
        /// 再生中の BGM をすべて停止します。
        /// </summary>
        void StopBGM();

        /// <summary>
        /// 再生中の SE をすべて停止します。
        /// </summary>
        void StopSE();
    }

    /// <summary>
    /// オーディオ機能の統括ファサード。  
    /// <para>
    /// - BGM / SE の再生・停止  
    /// - 音量・ミュートの管理  
    /// - AudioMixer との連携  
    /// を一括で扱う窓口クラスです。
    /// </para>
    /// </summary>
    public class AudioFacade : IAudioFacade
    {
        private readonly IBGMPlayer _bgmPlayer;
        private readonly ISEPlayer _sePlayer;
        private readonly IVolumeMediator _volumeMediator;
        private readonly IMuteMediator _muteMediator;

        /// <summary>
        /// 依存関係はすべて DI で注入されます。
        /// </summary>
        [Inject]
        public AudioFacade(
            IBGMPlayer bgmPlayer,
            ISEPlayer sePlayer,
            IVolumeMediator volumeMediator,
            IMuteMediator muteMediator)
        {
            _bgmPlayer = bgmPlayer;
            _sePlayer = sePlayer;
            _volumeMediator = volumeMediator;
            _muteMediator = muteMediator;
        }

        /// <inheritdoc />
        public IReadOnlyDictionary<SoundType, Volume> VolumeDict => _volumeMediator.VolumeDict;

        /// <inheritdoc />
        public IReadOnlyDictionary<SoundType, bool> MuteDict => _muteMediator.MuteDict;

        /// <inheritdoc />
        public void ChangeVolume(SoundType soundType, Volume volume)
            => _volumeMediator.ChangeVolume(soundType, volume);

        /// <inheritdoc />
        public void ToggleMute(SoundType soundType)
            => _muteMediator.ToggleMute(soundType);

        /// <inheritdoc />
        public async UniTask PlayAsync(BGMType bgm, CancellationToken ct = default)
            => await _bgmPlayer.PlayAsync(bgm, ct);

        /// <inheritdoc />
        public async UniTask PlayAsync(SEType se, CancellationToken ct = default)
            => await _sePlayer.PlayAsync(se, ct);

        /// <inheritdoc />
        public void StopBGM() => _bgmPlayer.Stop();

        /// <inheritdoc />
        public void StopSE() => _sePlayer.Stop();
    }
}
