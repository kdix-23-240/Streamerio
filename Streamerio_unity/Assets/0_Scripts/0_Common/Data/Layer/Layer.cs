using System;
using UnityEngine;

namespace Common
{
    /// <summary>
    /// レイヤー(参考：https://nekojara.city/unity-layer-inspector)
    /// </summary>
    [Serializable]
    public struct Layer
    {
        [SerializeField] private int _value;

        /// <summary>
        /// レイヤーインデックス
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public int Value
        {
            get => _value;
            set
            {
                if (value < 0 || value > 31)
                {
                    throw new ArgumentOutOfRangeException(nameof(value), value, "Value must be between 0 and 31.");
                }
                
                _value = value;
            }
        }

        /// <summary>
        /// レイヤー名
        /// </summary>
        /// <exception cref="ArgumentException"></exception>
        public string Name
        {
            get => LayerMask.LayerToName(Value);
            set
            {
                var layerValue = LayerMask.NameToLayer(value);
                if (layerValue == -1)
                {
                    throw new ArgumentException("Invalid layer name.", nameof(value));
                }
                
                _value = layerValue;
            }
        }

        /// <summary>
        /// レイヤーマスク
        /// </summary>
        public LayerMask Mask => 1 << _value;
        
        /// <summary>
        /// intに変換
        /// </summary>
        /// <param name="layer"></param>
        /// <returns></returns>
        public static implicit operator int(Layer layer) => layer.Value;
        /// <summary>
        /// Layerに変換
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static explicit operator Layer(int value) => new Layer { Value = value };
        /// <summary>
        /// stringに変換
        /// </summary>
        /// <returns></returns>
        public override string ToString() => $"{Name}({Value})";
    }
}