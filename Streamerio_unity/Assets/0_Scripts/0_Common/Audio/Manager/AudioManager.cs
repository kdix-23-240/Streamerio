using Alchemy.Inspector;
using System;
using System.Collections.Generic;
using System.Threading;

using Common.Save;

using Cysharp.Threading.Tasks;

using UnityEngine;
using UnityEngine.Audio;

namespace Common.Audio
{
    /// <summary>
    /// オーディオの操作を管理(注意：使用前に初期化関数を呼ぶ)
    /// </summary>
    public class AudioManager: SingletonBase<AudioManager>
    {
        [Header("AudioMixer")]
        [SerializeField]
        private AudioMixer _audioMixer;
        [SerializeField, LabelText("AudioMixerのパラメータを管理するSO")]
        private AudioMixerParameterSO _audioMixerParameterSO;
        
        [Header("BGM")]
        [SerializeField, LabelText("ScriptableObject")]
        private BGMScriptableObject _bgmSO;
        [SerializeField, LabelText("AudioSource")]
        private Source _bgmSource;
        [SerializeField, LabelText("AudioSourceのプールの容量"), Min(2)]
        private int _bgmSourcePoolCapacity = 3;

        [Header("SE")]
        [SerializeField, LabelText("ScriptableObject")]
        private SEScriptableObject _seSO;
        [SerializeField, LabelText("SEのAudioSource")]
        private Source _seSource;
        [SerializeField, LabelText("SEのAudioSourceのプールの容量"), Min(2)]
        private int _seSourcePoolCapacity = 10;

        /* オーディオミキサー */
        private AudioMixerController _audioMixerController;

        /* 曲 */
        private IReadOnlyDictionary<BGMType, AudioClip> _bgmDict;
        private IReadOnlyDictionary<SEType, AudioClip> _seDict;
        
        /* 音量 */
        private Dictionary<VolumeType, Volume> _volumeDict;
        /// <summary>
        /// 音量のデータ
        /// </summary>
        public IReadOnlyDictionary<VolumeType, Volume> VolumeDict => _volumeDict;

        private Dictionary<VolumeType, bool> _muteDict;
        /// <summary>
        /// ミュート状態
        /// </summary>
        public IReadOnlyDictionary<VolumeType, bool> MuteDict => _muteDict;

        /* オーディオソース */
        private AudioSourcePool _bgmAudioSourcePool;
        private AudioSourcePool _seAudioSourcePool;

        private HashSet<Source> _usedBGMSources;
        private HashSet<Source> _usedSESources;

        /// <summary>
        /// 初期化
        /// </summary>
        public void Initialize()
        {
            _audioMixerController = new AudioMixerController(_audioMixer, _audioMixerParameterSO);

            _bgmDict = _bgmSO.Dictionary;
            _seDict = _seSO.Dictionary;
            
            _volumeDict = new (SaveManager.Instance.LoadVolumes());
            _muteDict = new()
            {
                { VolumeType.Master, false },
                { VolumeType.BGM, false }, 
                { VolumeType.SE, false }, 
            };

            _bgmAudioSourcePool = new AudioSourcePool(_bgmSource, transform, _bgmSourcePoolCapacity);
            _seAudioSourcePool = new AudioSourcePool(_seSource, transform, _seSourcePoolCapacity);

            _usedBGMSources = new HashSet<Source>();
            _usedSESources = new HashSet<Source>();
        }

        /// <summary>
        /// 音量を変える
        /// </summary>
        /// <param name="volumeType"></param>
        /// <param name="volume"></param>
        public void ChangeVolume(VolumeType volumeType, Volume volume)
        {
            _audioMixerController.ChangeVolume(volumeType, volume);
            _volumeDict[volumeType]= volume;
        }

        /// <summary>
        /// BGMを鳴らして、終わるまで待つ
        /// </summary>
        /// <param name="bgm"></param>
        /// <param name="ct"></param>
        public async UniTask PlayAsync(BGMType bgm, CancellationToken ct)
        {
            await GetSource(_bgmAudioSourcePool, _usedBGMSources).PlayAsync(_bgmDict[bgm], ct);
        }

        /// <summary>
        /// SEを鳴らして、終わるまで待つ
        /// </summary>
        /// <param name="se"></param>
        /// <param name="ct"></param>
        public async UniTask PlayAsync(SEType se, CancellationToken ct)
        {
            await GetSource(_seAudioSourcePool, _usedSESources).PlayAsync(_seDict[se], ct);
        }

        /// <summary>
        /// BGMを停止する
        /// </summary>
        public void StopBGM()
        {
            foreach (var source in _usedBGMSources)
            {
                source.Stop();
            }
        }

        /// <summary>
        /// SEを停止する
        /// </summary>
        public void StopSE()
        {
            foreach (var source in _usedSESources)
            {
                source.Stop();
            }
        }

        /// <summary>
        /// ミュート設定
        /// </summary>
        public void ToggleMute(VolumeType type)
        {
            _muteDict[type] = !_muteDict[type];
            
            if (type == VolumeType.Master || type == VolumeType.BGM)
            {
                SetMute(_muteDict[type], _bgmSource, _usedBGMSources);
            }

            if (type == VolumeType.Master || type == VolumeType.SE)
            {
                SetMute(_muteDict[type], _seSource, _usedSESources);
            }
        }

        /// <summary>
        /// ミュート設定
        /// </summary>
        /// <param name="isMute"></param>
        /// <param name="musicSource"></param>
        /// <param name="usedSources"></param>
        private void SetMute(bool isMute, Source musicSource, HashSet<Source> usedSources)
        {
            musicSource.SetMute(isMute);

            foreach (var source in usedSources)
            {
                source.SetMute(isMute);
            }
        }

        /// <summary>
        /// AudioSourceを取得
        /// </summary>
        /// <returns></returns>
        private Source GetSource(AudioSourcePool pool, HashSet<Source> usedSources)
        {
            var source = pool.GetSorce();
            usedSources.Add(source);
            return source;
        }

        /// <summary>
        /// 音量をセーブ
        /// </summary>
        public void SaveVolumes()
        {
            foreach (VolumeType type in Enum.GetValues(typeof(VolumeType)))
            {
                SaveManager.Instance.SaveAudioVolume(type, _volumeDict[type]);
            }
        }
    }
}
