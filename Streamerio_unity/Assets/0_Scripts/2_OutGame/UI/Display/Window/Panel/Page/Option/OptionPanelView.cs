using System.Collections.Generic;
using Alchemy.Inspector;
using Common;
using Common.Audio;
using Common.UI.Display.Window.Panel;
using UnityEngine;
using UnityEngine.UI;

namespace OutGame.UI.Display.Window.Panel.Page.Option
{
    /// <summary>
    /// オプションパネルの見た目
    /// </summary>
    public class OptionPanelView:PagePanelView
    {
        [SerializeField, LabelText("サウンドスライダー")]
        private SerializeDictionary<VolumeType, Slider> _soundSliderDict;
        public IReadOnlyDictionary<VolumeType, Slider> SoundSliderDict => _soundSliderDict.ToDictionary();
    }
}