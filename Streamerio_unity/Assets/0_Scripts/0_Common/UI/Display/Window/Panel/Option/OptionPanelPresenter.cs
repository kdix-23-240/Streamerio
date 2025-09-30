using System.Threading;
using Alchemy.Inspector;
using Common.Audio;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Volume = Common.Audio.Volume;

namespace Common.UI.Display.Window.Panel
{
    /// <summary>
    /// オプションパネルの繋ぎ役
    /// </summary>
    [RequireComponent(typeof(OptionPanelView))]
    public class OptionPanelPresenter: PagePanelPresenter
    {
        [SerializeField, ReadOnly]
        private OptionPanelView _optionView;

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();
            _optionView ??= GetComponent<OptionPanelView>();
        }
#endif
        public override void Initialize()
        {
            base.Initialize();

            foreach (var slider in _optionView.SoundSliderDict)
            {
                slider.Value.minValue = Volume.MIN_VALUE;
                slider.Value.maxValue = Volume.MAX_VALUE;
                slider.Value.value = AudioManager.Instance.VolumeDict[slider.Key].Value;
                
                slider.Value.onValueChanged.AddListener(value =>
                {
                    AudioManager.Instance.ChangeVolume(slider.Key, new Volume(value));
                });
            }
        }

        public override async UniTask HideAsync(CancellationToken ct)
        {
            AudioManager.Instance.SaveVolumes();
            await base.HideAsync(ct);
        }

        public override void Hide()
        {
            AudioManager.Instance.SaveVolumes();
            base.Hide();
        }
    }
}