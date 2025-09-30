using System.Collections.Generic;
using Alchemy.Inspector;
using Common.Audio;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Common.UI.Display.Window.Panel
{
    /// <summary>
    /// オプションパネルの View（見た目担当）。
    /// - 音量設定用のスライダーを保持
    /// - VolumeType ごとに Slider を辞書管理し、Presenter 側からアクセス可能にする
    /// - 実際のスライダー操作の処理（値の反映や保存）は Presenter 側で行う
    /// </summary>
    public class OptionPagePanelView : UIBehaviour
    {
        [SerializeField, LabelText("サウンドスライダー")]
        private SerializeDictionary<VolumeType, Slider> _soundSliderDict;

        /// <summary>
        /// 音量スライダーを VolumeType ごとに公開（読み取り専用）
        /// </summary>
        public IReadOnlyDictionary<VolumeType, Slider> SoundSliderDict => _soundSliderDict.ToDictionary();
    }
}