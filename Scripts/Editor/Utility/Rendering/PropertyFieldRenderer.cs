using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NiceAttributes.Editor.PropertyDrawers_SpecialCase;
using UnityEditor;
using UnityEngine;

namespace NiceAttributes.Editor.Utility
{
    public static class PropertyFieldRenderer
    {
        public const float IndentLength = 15.0f;
        public const float HorizontalSpacing = 2.0f;

        private delegate void PropertyFieldFunction(Rect rect, SerializedProperty property, GUIContent label, bool includeChildren);

        public static void PropertyField(Rect rect, SerializedProperty property, bool includeChildren)
        {
            PropertyField_Implementation(rect, property, includeChildren, DrawPropertyField);
        }

        public static void PropertyField_Layout(SerializedProperty property, bool includeChildren)
        {
            Rect dummyRect = new Rect();
            PropertyField_Implementation(dummyRect, property, includeChildren, DrawPropertyField_Layout);
        }

        private static void DrawPropertyField(Rect rect, SerializedProperty property, GUIContent label, bool includeChildren)
        {
            EditorGUI.PropertyField(rect, property, label, includeChildren);
        }

        private static void DrawPropertyField_Layout(Rect rect, SerializedProperty property, GUIContent label, bool includeChildren)
        {
            EditorGUILayout.PropertyField(property, label, includeChildren);
        }

        private static void PropertyField_Implementation(Rect rect, SerializedProperty property,
            bool includeChildren, PropertyFieldFunction propertyFieldFunction)
        {
            var specialCaseAttribute = PropertyUtility.GetAttribute<SpecialCaseDrawerAttribute>(property);
            if (specialCaseAttribute != null)
            {
                specialCaseAttribute.GetDrawer().OnGUI(rect, property);
            }
            else
            {
                PropertyDrawPipeline.Execute(
                    rect,
                    property,
                    (drawRect, drawProperty, label) => propertyFieldFunction.Invoke(drawRect, drawProperty, label, includeChildren));
            }
        }

        public static float GetIndentLength(Rect sourceRect)
        {
            Rect indentRect = EditorGUI.IndentedRect(sourceRect);
            return indentRect.x - sourceRect.x;
        }
    }
}
