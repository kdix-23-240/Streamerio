using System.Collections.Generic;
using UnityEngine.Audio;

namespace Common.Audio
{
    /// <summary>
    /// AudioMixer の制御を担当するクラス。
    /// - VolumeType と AudioMixer のパラメータ名を紐づけ
    /// - 音量変更処理を統一的に呼び出せるようにする
    /// - 実際の AudioMixer.SetFloat() 呼び出しをカプセル化
    /// </summary>
    public class AudioMixerController
    {
        private readonly AudioMixer _audioMixer; 
        /// <summary>
        /// 各 VolumeType に対応する AudioMixer のパラメータ名を保持する辞書
        /// </summary>
        private readonly IReadOnlyDictionary<VolumeType, string> _volumeParameterDict;

        /// <summary>
        /// コンストラクタ。
        /// - AudioMixer と ParameterSO を受け取って初期化
        /// - SO に定義された VolumeType → パラメータ名の対応表を内部に保持
        /// </summary>
        public AudioMixerController(AudioMixer audioMixer, AudioMixerParameterSO parameterSO)
        {
            _audioMixer = audioMixer;
            _volumeParameterDict = parameterSO.VolumeParamDict;
        }

        /// <summary>
        /// 音量を変更する。
        /// - 指定された VolumeType に対応する AudioMixer のパラメータを更新
        /// - Volume から変換された MixerValue を AudioMixer にセット
        /// </summary>
        /// <param name="volumeType">音量の種類（Master / BGM / SE など）</param>
        /// <param name="volume">設定する音量データ</param>
        public void ChangeVolume(VolumeType volumeType, Volume volume)
        {
            _audioMixer.SetFloat(_volumeParameterDict[volumeType], volume.MixerValue);
        }
    }
}