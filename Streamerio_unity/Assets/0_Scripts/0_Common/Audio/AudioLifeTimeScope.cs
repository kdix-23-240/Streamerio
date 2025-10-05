using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using VContainer;
using VContainer.Unity;

namespace Common.Audio
{
    /// <summary>
    /// オーディオ周り（BGM/SE、プール、データ辞書など）の DI 登録をまとめた Scope。
    /// </summary>
    public class AudioLifeTimeScope : LifetimeScope
    {
        [Header("SoundTypeごとの再生用プレハブと親Transform")]
        [SerializeField] private SerializeDictionary<SoundType, SourceData> _sourceDataDict = new();

        [Header("BGM/SE のデータ辞書を持つ ScriptableObject")]
        [SerializeField] private BGMScriptableObject _bgmSO;
        [SerializeField] private SEScriptableObject _seSO;
        
        [Header("AudioMixer 本体")]
        [Tooltip("AudioMixer アセットへの参照。音量調整などの制御対象となる。")]
        [SerializeField]
        private AudioMixer _audioMixer;

        [Header("AudioMixer パラメータ設定")]
        [Tooltip("各 SoundType に対応する AudioMixer のパラメータ名を定義した ScriptableObject。")]
        [SerializeField]
        private AudioMixerParameterSO _parameterSO;

        protected override void Configure(IContainerBuilder builder)
        {
            // --- 再生データ辞書（読み取り専用で注入） ---
            builder.RegisterInstance(_bgmSO.Dictionary);
            builder.RegisterInstance(_seSO.Dictionary);

            // --- AudioSourcePool ファクトリ ---
            builder.RegisterFactory<SoundType, int, AudioSourcePool>(_ =>
                    (type, capacity) => new AudioSourcePool(
                        _sourceDataDict[type].Source,
                        _sourceDataDict[type].Parent,
                        capacity
                    ),
                Lifetime.Scoped
            );

            // --- AudioMixer 連携 ---
            builder.RegisterInstance(_audioMixer);
            builder.RegisterInstance(_parameterSO.VolumeParamDict);
            builder.Register<IVolumeController, AudioMixerMediator>(Lifetime.Singleton);

            // --- Mediator（重複登録しない：IStartableはEntryPointに任せる） ---
            builder.Register<VolumeMediator>(Lifetime.Singleton)
                .As<IVolumeMediator>()
                .As<IMuteMediator>()
                .As<IStartable>();

            // --- プレイヤ（BGM/SE） ---
            builder.Register<IBGMPlayer, BGMPlayer>(Lifetime.Singleton);
            builder.Register<ISEPlayer,  SEPlayer >(Lifetime.Singleton);

            // --- Facade ---
            builder.Register<IAudioFacade, AudioFacade>(Lifetime.Singleton);
        }

        [Serializable]
        private class SourceData
        {
            public Source    Source;
            public Transform Parent;
        }
    }
}