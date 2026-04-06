using System.Reflection;
using NiceAttributes.Model;
using UnityEditor;
using UnityEngine;

namespace NiceAttributes.Editor.Utility
{
    public static class DropdownRenderer
    {
        public static void Dropdown(Rect rect,
            SerializedObject serializedObject, object target, FieldInfo dropdownField,
            string label, int selectedValueIndex, object[] values, string[] displayOptions)
        {
            EditorGUI.BeginChangeCheck();

            int newIndex = EditorGUI.Popup(rect, label, selectedValueIndex, displayOptions);
            object newValue = values[newIndex];

            object dropdownValue = dropdownField.GetValue(target);
            if (dropdownValue == null || !dropdownValue.Equals(newValue))
            {
                Undo.RecordObject(serializedObject.targetObject, "Dropdown");
                dropdownField.SetValue(target, newValue);
            }
        }
    }
}
