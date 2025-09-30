using System.Collections.Generic;
using Alchemy.Inspector;
using Common.Audio;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Common.UI.Display.Window.Panel
{
    /// <summary>
    /// オプションパネルの見た目
    /// </summary>
    public class OptionPanelView:UIBehaviour
    {
        [SerializeField, LabelText("サウンドスライダー")]
        private SerializeDictionary<VolumeType, Slider> _soundSliderDict;
        public IReadOnlyDictionary<VolumeType, Slider> SoundSliderDict => _soundSliderDict.ToDictionary();
    }
}