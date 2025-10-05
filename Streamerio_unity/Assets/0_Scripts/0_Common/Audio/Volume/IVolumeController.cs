namespace Common.Audio
{
    /// <summary>
    /// 音量変更を外部から制御するためのインターフェイス。  
    /// <para>
    /// 実装クラスは、AudioMixer などに対して実際の音量反映処理を行います。
    /// </para>
    /// </summary>
    public interface IVolumeController
    {
        /// <summary>
        /// 指定したサウンドタイプの音量を変更します。
        /// </summary>
        /// <param name="soundType">音量を変更する対象のサウンドタイプ。</param>
        /// <param name="volume">設定する音量値。</param>
        void ChangeVolume(SoundType soundType, Volume volume);
    }
}