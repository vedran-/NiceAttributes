using System;
using System.Collections;
using UnityEditor;
using UnityEngine;

namespace NiceAttributes.Editor
{
    [CustomPropertyDrawer(typeof(IList), true)]
    [CustomPropertyDrawer(typeof(Array), true)]
    public class ListPropertyDrawer : PropertyDrawerBase
    {
        protected override float GetPropertyHeight_Internal(SerializedProperty property, GUIContent label)
        {
            if (property.isArray)
                return EditorGUI.GetPropertyHeight(property, label, true);
            else
                return base.GetPropertyHeight(property) + GetHelpBoxHeight();
        }

        protected override void OnGUI_Internal(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);
            
            EditorGUILayout.LabelField("HELLOOO");

            if (property.isArray)
            {
                EditorGUI.PropertyField(position, property, label, true);
            }
            else
            {
                string message = "Property is not an IList.";
                EditorGUI.HelpBox(position, message, MessageType.Warning);
            }

            EditorGUI.EndProperty();
        }
    }
}