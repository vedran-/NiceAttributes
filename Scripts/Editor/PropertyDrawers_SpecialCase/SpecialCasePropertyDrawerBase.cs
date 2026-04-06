using NiceAttributes.Editor.Utility;
using UnityEditor;
using UnityEngine;

namespace NiceAttributes.Editor.PropertyDrawers_SpecialCase
{
    public abstract class SpecialCasePropertyDrawerBase
    {
        public void OnGUI(Rect rect, SerializedProperty property)
        {
            PropertyDrawPipeline.Execute(rect, property, OnGUI_Internal);
        }

        public float GetPropertyHeight(SerializedProperty property)
        {
            return GetPropertyHeight_Internal(property);
        }

        protected abstract void OnGUI_Internal(Rect rect, SerializedProperty property, GUIContent label);
        protected abstract float GetPropertyHeight_Internal(SerializedProperty property);
    }
}
