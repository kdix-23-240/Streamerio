using System;
using UnityEngine;

namespace Common
{
    /// <summary>
    /// 位置、角度、サイズのみを持つトランスフォーム
    /// </summary>
    [Serializable]
    public class SimpleTransform
    {
        [SerializeField, Header("位置")]
        private Vector3 _position;
        public Vector3 Position => _position;
        [SerializeField, Header("角度")]
        private Vector3 _rotation;
        public Vector3 Rotation => _rotation;
        [SerializeField, Header("スケール")]
        private Vector3 _scale;
        public Vector3 Scale => _scale;

        public SimpleTransform(Vector3 position, Vector3 rotation, Vector3 scale)
        {
            _position = position;
            _rotation = rotation;
            _scale = scale;
        }

        /// <summary>
        /// Transformに反映する
        /// </summary>
        /// <param name="transform"></param>
        public void Apply(Transform transform)
        {
            transform.position = _position;
            transform.eulerAngles = _rotation;
            transform.localScale = _scale;
        }

        /// <summary>
        /// Transformのローカルに反映
        /// </summary>
        /// <param name="transform"></param>
        public void ApplyLocal(Transform transform)
        {
            transform.localPosition = _position;
            transform.localEulerAngles = _rotation;
            transform.localScale = _scale;
        }
    }
}