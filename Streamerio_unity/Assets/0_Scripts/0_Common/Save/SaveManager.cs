using System;
using System.Collections.Generic;
using Alchemy.Inspector;
using Common.Audio;

using UnityEngine;

namespace Common.Save
{
    /// <summary>
    /// セーブ/ロードを管理する
    /// </summary>
    public class SaveManager : SingletonBase<SaveManager>
    {
        [SerializeField, LabelText("一度遊んだかのキー名")]
        private string _playedKey = "Played";

        public bool IsRetry = false;
        
        /// <summary>
        /// 一度遊んだことを保存
        /// </summary>
        public void SavePlayed(bool isPlayed = true)
        {
            PlayerPrefs.SetInt(_playedKey, isPlayed ? 1 : 0);
        }
        
        /// <summary>
        /// 一度遊んだかロード
        /// </summary>
        /// <returns></returns>
        public bool LoadPlayed()
        {
            return PlayerPrefs.GetInt(_playedKey, 0) == 1;
        }
        
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
                Debug.Log($"Load Volume: {type} = {volumeValue}");
            }

            return volumes;
        }
    }
}