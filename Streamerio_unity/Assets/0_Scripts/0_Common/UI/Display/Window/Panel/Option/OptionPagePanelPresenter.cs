using System.Threading;
using Alchemy.Inspector;
using Common.Audio;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Volume = Common.Audio.Volume;

namespace Common.UI.Display.Window.Panel
{
    /// <summary>
    /// オプションパネルの Presenter。
    /// - サウンド系の設定スライダーを AudioManager とバインド
    /// - スライダーの値をリアルタイムに AudioManager に反映
    /// - パネルを閉じる際にボリューム設定を保存
    /// </summary>
    [RequireComponent(typeof(OptionPagePanelView))]
    public class OptionPagePanelPresenter : PagePanelPresenter
    {
        [SerializeField, ReadOnly]
        private OptionPagePanelView _view;

#if UNITY_EDITOR
        /// <summary>
        /// エディタ上で OptionPanelView を自動補完
        /// </summary>
        protected override void OnValidate()
        {
            base.OnValidate();
            _view ??= GetComponent<OptionPagePanelView>();
        }
#endif

        /// <summary>
        /// 初期化処理。
        /// - 各サウンドスライダーを AudioManager の VolumeDict と同期
        /// - スライダー操作時に音量を即時反映するリスナーを登録
        /// </summary>
        public override void Initialize()
        {
            base.Initialize();

            foreach (var slider in _view.SoundSliderDict)
            {
                // スライダーの範囲を Volume の定義に合わせる
                slider.Value.minValue = Volume.MIN_VALUE;
                slider.Value.maxValue = Volume.MAX_VALUE;
                // 初期値を現在の AudioManager の設定から反映
                slider.Value.value = AudioManager.Instance.VolumeDict[slider.Key].Value;
                
                // スライダー操作時に音量を変更
                slider.Value.onValueChanged.AddListener(value =>
                {
                    AudioManager.Instance.ChangeVolume(slider.Key, new Volume(value));
                });
            }
        }

        /// <summary>
        /// 非同期で非表示にする際、ボリュームを保存してから閉じる
        /// </summary>
        public override async UniTask HideAsync(CancellationToken ct)
        {
            AudioManager.Instance.SaveVolumes();
            await base.HideAsync(ct);
        }

        /// <summary>
        /// 即時非表示にする際、ボリュームを保存してから閉じる
        /// </summary>
        public override void Hide()
        {
            AudioManager.Instance.SaveVolumes();
            base.Hide();
        }
    }
}