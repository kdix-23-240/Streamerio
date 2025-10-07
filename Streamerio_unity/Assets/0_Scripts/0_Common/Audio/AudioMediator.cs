using System;
using Common.Save;
using UnityEngine;
using UnityEngine.Audio;
using VContainer;

namespace Common.Audio
{
    /// <summary>
    /// <para>Audio 関連（BGM/SE、AudioMixer、Volume/Mute Mediator など）をまとめて管理するObject。</para>
    /// <para>
    /// VContainer の DI により SaveManager を受け取り、  
    /// AudioMixer・各種 Mediator・Player の初期化を行い、  
    /// 外部に参照可能なプロパティとして公開する。
    /// </para>
    /// </summary>
    public class AudioMediator : MonoBehaviour
    {
        [Header("AudioMixer本体")]
        [Tooltip("音量調整の対象となる AudioMixer アセット。")]
        [SerializeField] 
        private AudioMixer _audioMixer;
        
        [Header("AudioMixer パラメータ定義")]
        [Tooltip("各 SoundType に対応する AudioMixer の Volume パラメータ名辞書を持つ ScriptableObject。")]
        [SerializeField] 
        private AudioMixerParameterSO _audioMixerParameterSO;

        [Header("SoundTypeごとのAudioSourceプレハブと親Transform")]
        [Tooltip("各 SoundType に対応する Source プレハブと Transform の辞書。")]
        [SerializeField] 
        private SerializeDictionary<SoundType, SourceData> _sourceDict;

        [Header("BGM/SE データ辞書")]
        [Tooltip("BGM 用の再生データ辞書 ScriptableObject。")]
        [SerializeField] 
        private BGMScriptableObject _bgmScriptableObject;

        [Tooltip("SE 用の再生データ辞書 ScriptableObject。")]
        [SerializeField] 
        private SEScriptableObject _seScriptableObject;
        
        // --- Mediator / Player の参照 ---
        private IVolumeMediator _volumeMediator;
        public IVolumeMediator VolumeMediator => _volumeMediator;

        private IMuteMediator _muteMediator;
        public IMuteMediator MuteMediator => _muteMediator;

        private IBGMPlayer _bgmPlayer;
        public IBGMPlayer BGMPlayer => _bgmPlayer;

        private ISEPlayer _sePlayer;
        public ISEPlayer SEPlayer => _sePlayer;

        /// <summary>
        /// VContainer から IVolumeSaveFacade を注入し、
        /// Audio 系の各コンポーネントを初期化する。
        /// </summary>
        /// <param name="saveManager">音量設定の永続化を担う SaveFacade。</param>
        [Inject]
        public void Constructor(IVolumeSaveFacade saveManager)
        {
            // --- AudioMixer と VolumeMediator の構築 ---
            var audioMixerMediator = new AudioMixerMediator(
                _audioMixer,
                _audioMixerParameterSO.VolumeParamDict
            );

            var volumeMediator = new VolumeMediator(saveManager, audioMixerMediator);
            _volumeMediator = volumeMediator;
            _muteMediator = (IMuteMediator)_volumeMediator;

            // --- AudioSourcePoolFactory の構築 ---
            var audioSourcePoolFactory = new AudioSourcePoolFactory(_sourceDict.ToDictionary());

            // --- BGM / SE Player の初期化 ---
            _bgmPlayer = new BGMPlayer(
                _bgmScriptableObject.Dictionary,
                audioSourcePoolFactory.Create
            );

            _sePlayer = new SEPlayer(
                _seScriptableObject.Dictionary,
                audioSourcePoolFactory.Create
            );
        }
    }
}