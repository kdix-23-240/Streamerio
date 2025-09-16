using System;
using System.Collections.Generic;

using Common.Audio;

using UnityEngine;

namespace Common.Save
{
    /// <summary>
    /// セーブ/ロードを管理する
    /// </summary>
    public class SaveManager : SingletonBase<SaveManager>
    {
        /// <summary>
        /// 音量をPlayerPrefsに保存(Enum名で保存)
        /// </summary>
        /// <param name="volumeType"></param>
        /// <param name="volume"></param>
        public void SaveAudioVolume(VolumeType volumeType, Volume volume)
        {
            PlayerPrefs.SetFloat(volumeType.ToString(), volume.Value);
        }

        /// <summary>
        /// 音量を全取得
        /// </summary>
        /// <returns></returns>
        public Dictionary<VolumeType, Volume> LoadVolumes()
        {
            var volumes = new Dictionary<VolumeType, Volume>();

            foreach (VolumeType type in Enum.GetValues(typeof(VolumeType)))
            {
                float volumeValue = PlayerPrefs.GetFloat(type.ToString(), Volume.DEFAULT_VALUE);
                volumes[type] = new Volume(volumeValue);
            }

            return volumes;
        }
    }
}