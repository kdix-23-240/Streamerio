using System;
using System.Linq;
using Common.UI;
using Cysharp.Text;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace InGame.UI.Timer
{
    public class TimerView: UIBehaviourBase
    {
        [SerializeField]
        private TMP_Text _timerText;

        /// <summary>
        /// ToStringの0埋め
        /// </summary>
        private string _zero;

        /// <summary>
        /// 初期化
        /// </summary>
        public void Initialize(float value)
        {
            base.Initialize();

            int zeroCount = Digit(value);
            char[] zeros = Enumerable.Repeat('0',zeroCount).ToArray();
            _zero = new string(zeros);
        }
        
        /// <summary>
        /// タイマーの見た目更新
        /// </summary>
        /// <param name="value"></param>
        public void UpdateTimerView(float value)
        {
            _timerText.SetText(value.ToString(_zero));
        }
        
        /// <summary>
        /// 桁数取得
        /// </summary>
        /// <param name="num"></param>
        /// <returns></returns>
        public int Digit(float num)
        {
            return (num == 0) ? 1 : ((int)Mathf.Log10(num) + 1);
        }
    }
}