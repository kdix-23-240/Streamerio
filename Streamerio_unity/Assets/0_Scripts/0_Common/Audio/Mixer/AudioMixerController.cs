using System.Collections.Generic;

using UnityEngine.Audio;

namespace Common.Audio
{
    /// <summary>
    /// オーディオミキサーを操作する
    /// </summary>
    public class AudioMixerController
    {
        private readonly AudioMixer _audioMixer;
        private readonly IReadOnlyDictionary<VolumeType, string> _volumeParameterDict;

        public AudioMixerController(AudioMixer audioMixer, AudioMixerParameterSO parameterSO)
        {
            _audioMixer = audioMixer;
            _volumeParameterDict = parameterSO.VolumeParamDict;
        }

        /// <summary>
        /// 音量を変える
        /// </summary>
        /// <param name="volumeType"></param>
        /// <param name="volume"></param>
        public void ChangeVolume(VolumeType volumeType, Volume volume)
        {
            _audioMixer.SetFloat(_volumeParameterDict[volumeType], volume.MixerValue);
        }
    }
}
