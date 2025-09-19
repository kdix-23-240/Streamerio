using Alchemy.Inspector;
using System.Collections.Generic;
using UnityEngine;

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
