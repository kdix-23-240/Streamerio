using System;
using System.Collections.Generic;
using Common.Save;
using VContainer;
using VContainer.Unity;

namespace Common.Audio
{
    /// <summary>
    /// 各サウンドタイプの音量を仲介・制御するインターフェイス。
    /// </summary>
    public interface IVolumeMediator
    {
        /// <summary>
        /// サウンドタイプごとの音量情報を保持する辞書（読み取り専用）。
        /// </summary>
        IReadOnlyDictionary<SoundType, Volume> VolumeDict { get; }

        /// <summary>
        /// 指定したサウンドタイプの音量を変更し、即時に適用します。
        /// </summary>
        /// <param name="type">対象のサウンドタイプ。</param>
        /// <param name="volume">設定する音量値。</param>
        void ChangeVolume(SoundType type, Volume volume);
    }
    
    /// <summary>
    /// サウンドタイプごとのミュート状態を仲介・制御するインターフェイス。
    /// </summary>
    public interface IMuteMediator
    {
        /// <summary>
        /// サウンドタイプごとのミュート状態（true = ミュート中）。
        /// </summary>
        IReadOnlyDictionary<SoundType, bool> MuteDict { get; }

        /// <summary>
        /// 指定したサウンドタイプのミュート状態をトグルします。
        /// </summary>
        /// <param name="type">ミュートを切り替える対象のサウンドタイプ。</param>
        void ToggleMute(SoundType type);
    }
    
    /// <summary>
    /// 音量・ミュート設定を管理し、<see cref="SaveManager"/> と <see cref="IVolumeController"/> の間で
    /// データを仲介するクラス。
    /// <para>
    /// 起動時にセーブデータから音量設定を読み込み、AudioMixer に適用します。  
    /// 実行中の音量変更・ミュート操作は AudioMixer に即時反映され、終了時には音量設定がセーブされます。
    /// </para>
    /// </summary>
    public class VolumeMediator : IStartable, IDisposable, IVolumeMediator, IMuteMediator
    {
        /// <summary>
        /// ミュート時に適用される音量値。
        /// </summary>
        private readonly Volume _mutedVolume = new(Volume.MIN_VALUE);
        
        /// <summary>
        /// 音量設定のセーブ・ロードを行うマネージャ。
        /// </summary>
        private readonly IVolumeSaveFacade _saveManager;

        /// <summary>
        /// 実際に AudioMixer へ音量値を適用するためのコントローラ。
        /// </summary>
        private readonly IVolumeController _volumeController;
        
        // --- 音量管理 ---
        
        /// <summary>
        /// サウンドタイプごとの音量データを保持する辞書。
        /// </summary>
        private Dictionary<SoundType, Volume> _volumeDict;
        
        /// <inheritdoc />
        public IReadOnlyDictionary<SoundType, Volume> VolumeDict => _volumeDict;
        
        // --- ミュート管理 ---
        
        /// <summary>
        /// サウンドタイプごとのミュート状態を保持する辞書。
        /// </summary>
        private Dictionary<SoundType, bool> _muteDict;
        
        /// <inheritdoc />
        public IReadOnlyDictionary<SoundType, bool> MuteDict => _muteDict;

        /// <summary>
        /// 依存関係を受け取るコンストラクタ。
        /// </summary>
        /// <param name="saveManager">音量データの保存・読み込みを行う。</param>
        /// <param name="volumeController">AudioMixer への音量適用を行うコントローラ。</param>
        [Inject]
        public VolumeMediator(IVolumeSaveFacade saveManager, IVolumeController volumeController)
        {
            _saveManager = saveManager;
            _volumeController = volumeController;
            
            _volumeDict = new Dictionary<SoundType, Volume>
            {
                { SoundType.Master, new Volume(Volume.DEFAULT_VALUE) },
                { SoundType.BGM,    new Volume(Volume.DEFAULT_VALUE) },
                { SoundType.SE,     new Volume(Volume.DEFAULT_VALUE) },
            };
            _muteDict = new Dictionary<SoundType, bool>
            {
                { SoundType.Master, false },
                { SoundType.BGM,    false },
                { SoundType.SE,     false },
            };
        }

        /// <summary>
        /// 起動時に呼び出され、セーブデータから音量設定を読み込み、
        /// ミュート状態を初期化したうえで AudioMixer に適用します。
        /// </summary>
        void IStartable.Start()
        {
            // セーブデータから音量情報を復元
            _volumeDict = new(_saveManager.LoadVolumes());
            
            // 音量をすべて AudioMixer に適用
            ApplyVolumeToAll();
        }
        
        /// <summary>
        /// 破棄時に現在の音量設定をセーブデータへ保存します。
        /// </summary>
        public void Dispose()
        {
            SaveVolumes();
        }
        
        /// <summary>
        /// 指定したサウンドタイプの音量を変更し、AudioMixer に即時反映します。
        /// </summary>
        /// <param name="type">対象のサウンドタイプ。</param>
        /// <param name="volume">設定する音量値。</param>
        public void ChangeVolume(SoundType type, Volume volume)
        {
            _volumeDict[type] = volume;
            ApplyVolume(type);
        }
        
        /// <summary>
        /// すべてのサウンドタイプに対して音量設定を適用します。
        /// </summary>
        private void ApplyVolumeToAll()
        {
            foreach (SoundType type in Enum.GetValues(typeof(SoundType)))
            {
                ApplyVolume(type);
            }
        }
        
        /// <summary>
        /// 指定したサウンドタイプの音量を、ミュート状態を考慮して AudioMixer に反映します。
        /// </summary>
        /// <param name="type">対象のサウンドタイプ。</param>
        private void ApplyVolume(SoundType type)
        {
            _volumeController.ChangeVolume(
                type,
                _muteDict[type] ? _mutedVolume : _volumeDict[type]
            );
        }
        
        /// <summary>
        /// 指定したサウンドタイプのミュート状態を切り替えます。
        /// - Master をミュートすると全体に影響します。
        /// - BGM / SE は個別に切り替え可能です。
        /// </summary>
        /// <param name="type">ミュート状態を切り替える対象のサウンドタイプ。</param>
        public void ToggleMute(SoundType type)
        {
            _muteDict[type] = !_muteDict[type];
            ApplyVolume(type);
        }
        
        /// <summary>
        /// 現在の音量設定をすべてセーブデータへ保存します。
        /// </summary>
        private void SaveVolumes()
        {
            foreach (SoundType type in Enum.GetValues(typeof(SoundType)))
            {
                _saveManager.SaveAudioVolume(type, _volumeDict[type]);
            }
        }
    }
}
