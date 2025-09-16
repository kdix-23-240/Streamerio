using Alchemy.Inspector;
using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using UnityEngine.Serialization;

namespace Common.Audio
{
    /// <summary>
    /// オーディオミキサーのパラメータ名
    /// </summary>
    [CreateAssetMenu(fileName = "AudioMixerParameterSO", menuName = "SO/Audio/AudioMixerParameter")]
    public class AudioMixerParameterSO: ScriptableObject
    {
        [SerializeField, LabelText("オーディオミキサーの音量パラメータ名")]
        private SerializeDictionary<VolumeType, string> _volumeParamDict;

        /// <summary>
        /// オーディオミキサーの音量パラメータ名
        /// </summary>
        public IReadOnlyDictionary<VolumeType, string> VolumeParamDict => _volumeParamDict.ToDictionary();
    }
}
