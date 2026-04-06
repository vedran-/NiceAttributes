using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace NiceAttributes.Editor.Utility
{
    public static class NiceEditorGUI
    {
        #region PropertyField delegation
        public static void PropertyField(Rect rect, SerializedProperty property, bool includeChildren)
            => PropertyFieldRenderer.PropertyField(rect, property, includeChildren);

        public static void PropertyField_Layout(SerializedProperty property, bool includeChildren)
            => PropertyFieldRenderer.PropertyField_Layout(property, includeChildren);

        public static float GetIndentLength(Rect sourceRect)
            => PropertyFieldRenderer.GetIndentLength(sourceRect);
        #endregion

        #region Dropdown delegation
        public static void Dropdown(Rect rect,
            SerializedObject serializedObject, object target, FieldInfo dropdownField,
            string label, int selectedValueIndex, object[] values, string[] displayOptions)
            => DropdownRenderer.Dropdown(rect, serializedObject, target, dropdownField, label, selectedValueIndex, values, displayOptions);
        #endregion

        #region Button delegation
        public static void Button(object target, MethodInfo methodInfo)
            => ButtonRenderer.Button(target, methodInfo);
        #endregion

        #region Non-serialized field delegation
        internal static void NativeProperty_Layout(object target, PropertyInfo property)
            => NonSerializedFieldRenderer.NativeProperty_Layout(target, property);

        internal static void NonSerializedField_Layout(object target, FieldInfo field)
            => NonSerializedFieldRenderer.NonSerializedField_Layout(target, field);
        #endregion

        #region HorizontalLine
        public static void HorizontalLine(Rect rect, float height, Color color)
        {
            rect.height = height;
            EditorGUI.DrawRect(rect, color);
        }
        #endregion

        #region HelpBox delegation
        public static void HelpBox(Rect rect, string message, MessageType messageType,
            UnityEngine.Object context = null, bool logToConsole = false)
            => HelpBoxRenderer.HelpBox(rect, message, messageType, context, logToConsole);

        public static void HelpBox_Layout(string message, MessageType messageType,
            UnityEngine.Object context = null, bool logToConsole = false)
            => HelpBoxRenderer.HelpBox_Layout(message, messageType, context, logToConsole);
        #endregion
    }
}
