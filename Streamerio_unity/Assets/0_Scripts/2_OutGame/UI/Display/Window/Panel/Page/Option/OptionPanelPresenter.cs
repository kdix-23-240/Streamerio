using Alchemy.Inspector;
using Common.Audio;
using Common.UI.Display.Window.Panel;
using UnityEngine;
using Volume = Common.Audio.Volume;

namespace OutGame.UI.Display.Window.Panel.Page.Option
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
            _optionView.Initialize();
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
    }
}