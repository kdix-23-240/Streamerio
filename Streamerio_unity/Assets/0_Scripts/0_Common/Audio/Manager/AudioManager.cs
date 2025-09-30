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
    /// オーディオの統括マネージャ。
    /// - BGM / SE の再生・停止・音量管理を一括で制御
    /// - AudioMixer 経由で音量・ミュート制御を行う
    /// - AudioSourcePool を利用して複数音の同時再生に対応
    /// - 使用前に必ず Initialize を呼ぶこと
    /// </summary>
    public class AudioManager : SingletonBase<AudioManager>
    {
        [Header("AudioMixer")]
        [SerializeField] private AudioMixer _audioMixer;
        [SerializeField, LabelText("AudioMixerのパラメータを管理するSO")]
        private AudioMixerParameterSO _audioMixerParameterSO;
        
        [Header("BGM")]
        [SerializeField, LabelText("ScriptableObject")]
        private BGMScriptableObject _bgmSO;
        [SerializeField, LabelText("AudioSource（BGM用の雛形）")]
        private Source _bgmSource;
        [SerializeField, LabelText("BGM AudioSourceプール容量"), Min(2)]
        private int _bgmSourcePoolCapacity = 3;

        [Header("SE")]
        [SerializeField, LabelText("ScriptableObject")]
        private SEScriptableObject _seSO;
        [SerializeField, LabelText("AudioSource（SE用の雛形）")]
        private Source _seSource;
        [SerializeField, LabelText("SE AudioSourceプール容量"), Min(2)]
        private int _seSourcePoolCapacity = 10;

        /* AudioMixer 制御 */
        private AudioMixerController _audioMixerController;

        /* 各種辞書 */
        private IReadOnlyDictionary<BGMType, AudioClip> _bgmDict;
        private IReadOnlyDictionary<SEType, AudioClip> _seDict;
        
        /* 音量管理 */
        private Dictionary<VolumeType, Volume> _volumeDict;
        public IReadOnlyDictionary<VolumeType, Volume> VolumeDict => _volumeDict;

        /* ミュート管理 */
        private Dictionary<VolumeType, bool> _muteDict;
        public IReadOnlyDictionary<VolumeType, bool> MuteDict => _muteDict;

        /* AudioSource プール管理 */
        private AudioSourcePool _bgmAudioSourcePool;
        private AudioSourcePool _seAudioSourcePool;

        /* 使用中の AudioSource を記録 */
        private HashSet<Source> _usedBGMSources;
        private HashSet<Source> _usedSESources;

        /// <summary>
        /// 初期化処理。
        /// - AudioMixerController を生成
        /// - ScriptableObject から BGM / SE の辞書を構築
        /// - セーブデータから音量を復元して適用
        /// - AudioSourcePool を作成
        /// </summary>
        public void Initialize()
        {
            _audioMixerController = new AudioMixerController(_audioMixer, _audioMixerParameterSO);

            _bgmDict = _bgmSO.Dictionary;
            _seDict = _seSO.Dictionary;
            
            _volumeDict = new(SaveManager.Instance.LoadVolumes());
            _muteDict = new()
            {
                { VolumeType.Master, false },
                { VolumeType.BGM, false }, 
                { VolumeType.SE, false }, 
            };
            
            // 音量適用
            foreach (VolumeType type in Enum.GetValues(typeof(VolumeType)))
            {
                ChangeVolume(type, _volumeDict[type]);
            }

            // プール生成
            _bgmAudioSourcePool = new AudioSourcePool(_bgmSource, transform, _bgmSourcePoolCapacity);
            _seAudioSourcePool = new AudioSourcePool(_seSource, transform, _seSourcePoolCapacity);

            _usedBGMSources = new HashSet<Source>();
            _usedSESources = new HashSet<Source>();
        }

        /// <summary>
        /// 音量を変更する。
        /// - AudioMixer に反映し、内部辞書も更新
        /// </summary>
        public void ChangeVolume(VolumeType volumeType, Volume volume)
        {
            _audioMixerController.ChangeVolume(volumeType, volume);
            _volumeDict[volumeType] = volume;
        }

        /// <summary>
        /// BGM を再生（終了まで待機）。
        /// </summary>
        public async UniTask PlayAsync(BGMType bgm, CancellationToken ct)
        {
            if (bgm == BGMType.None)
            {
                return;
            }
            
            await GetSource(_bgmAudioSourcePool, _usedBGMSources).PlayAsync(_bgmDict[bgm], ct);
        }

        /// <summary>
        /// SE を再生（終了まで待機）。
        /// </summary>
        public async UniTask PlayAsync(SEType se, CancellationToken ct)
        {
            if (se == SEType.None)
            {
                return;
            }
            
            await GetSource(_seAudioSourcePool, _usedSESources).PlayAsync(_seDict[se], ct);
        }

        /// <summary>
        /// 再生中の BGM を全て停止。
        /// </summary>
        public void StopBGM()
        {
            foreach (var source in _usedBGMSources)
            {
                source.Stop();
            }
        }

        /// <summary>
        /// 再生中の SE を全て停止。
        /// </summary>
        public void StopSE()
        {
            foreach (var source in _usedSESources)
            {
                source.Stop();
            }
        }

        /// <summary>
        /// 音量タイプごとにミュート切り替え。
        /// - Master をミュートすると全体に影響
        /// - BGM / SE 個別にも切替可能
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
        /// 指定ソース群にミュートを適用。
        /// </summary>
        private void SetMute(bool isMute, Source templateSource, HashSet<Source> usedSources)
        {
            templateSource.SetMute(isMute);

            foreach (var source in usedSources)
            {
                source.SetMute(isMute);
            }
        }

        /// <summary>
        /// プールから Source を取得して使用中リストに追加。
        /// </summary>
        private Source GetSource(AudioSourcePool pool, HashSet<Source> usedSources)
        {
            var source = pool.GetSource();
            usedSources.Add(source);
            return source;
        }

        /// <summary>
        /// 音量をセーブデータに保存。
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
