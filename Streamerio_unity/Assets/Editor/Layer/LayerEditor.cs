using UnityEditor;
using UnityEngine;
using Common;

namespace Infra
{
    [CustomPropertyDrawer(typeof(Layer))]
    public class LayerPropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);
            
            var valueProperty = property.FindPropertyRelative("_value");
            
            // 現在のレイヤーインデックス
            var currentValue = valueProperty.intValue;
            
            // レイヤーインデックス更新
            var newValue = EditorGUI.LayerField(position, label, currentValue);
            valueProperty.intValue = newValue;
            
            EditorGUI.EndProperty();
        }
    }
}