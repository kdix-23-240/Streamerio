using System;
using UnityEngine;

namespace Common.Audio
{
    /// <summary>
    /// 音量
    /// </summary>
    [Serializable]
    public class Volume
    {
        /// <summary>
        /// 入力として受け取れる音量のデフォルト値
        /// </summary>
        public const float DEFAULT_VALUE = 50;

        // 入力として受け取れる最小値・最大値
        public const float MIN_VALUE = 0;
        public const float MAX_VALUE = 100;

        // オーディオミキサーの最小値・最大値
        private const float MIN_MIXER_VOLUME = -80f;
        private const float MAX_MIXER_VOLUME = 20f;

        // オーディオミキサーに設定できる最大値(オーディオミキサーの上限まで上げると大きすぎるため)
        private const float CUSTOM_MAX_VOLUME = 0f;

        [SerializeField, Header("音量"), Range(MIN_VALUE, MAX_VALUE)]
        private float _value = 0f;
        /// <summary>
        /// 0~100の音量(UIとかで使う用)
        /// </summary>
        public float Value => _value;

        private float _mixerValue;
        /// <summary>
        /// オーディオミキサーに設定する音量
        /// </summary>
        public float MixerValue => _mixerValue;

#if UNITY_EDITOR
        private void OnValidate()
        {
            _mixerValue = ConvertVolume2dB(_value);
        }
#endif

        /// <summary>
        /// 音量
        /// <param>入力は強制的に0~100の値になる</param>
        /// </summary>
        /// <param name="value"> 0~100の音量 </param>
        public Volume(float value)
        {
            _value = Mathf.Clamp(value, MIN_VALUE, MAX_VALUE);
            _mixerValue = ConvertVolume2dB(value);
        }

        /// <summary>
        /// 音量をデシベルに変換(オーディオミキサーに設定する音量に変換)
        /// </summary>
        /// <param name="volume"></param>
        /// <returns></returns>
        private float ConvertVolume2dB(float volume)
            => Mathf.Clamp(MAX_MIXER_VOLUME * Mathf.Log10(ScaleToZeroToOne(volume)), MIN_MIXER_VOLUME, CUSTOM_MAX_VOLUME);

        /// <summary>
        /// 音量を0~1の間にする
        /// </summary>
        /// <param name="volume"></param>
        /// <returns></returns>
        private float ScaleToZeroToOne(float volume)
            => Mathf.Clamp(volume, MIN_VALUE, MAX_VALUE) / MAX_VALUE;
    }
}
