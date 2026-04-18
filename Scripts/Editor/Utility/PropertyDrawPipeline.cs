using System;
using NiceAttributes.Editor.PropertyValidators;
using NiceAttributes.Model;
using UnityEditor;
using UnityEngine;

namespace NiceAttributes.Editor.Utility
{
    public static class PropertyDrawPipeline
    {
        public static object CurrentTarget { get; internal set; }

        public static void Execute(Rect rect, SerializedProperty property, Action<Rect, SerializedProperty, GUIContent> drawProperty)
        {
            bool visible = PropertyUtility.IsVisible(property);
            if (!visible)
            {
                return;
            }

            ValidatorAttribute[] validatorAttributes = PropertyUtility.GetAttributes<ValidatorAttribute>(property);
            foreach (var validatorAttribute in validatorAttributes)
            {
                validatorAttribute.GetValidator().ValidateProperty(property);
            }

            EditorGUI.BeginChangeCheck();
            bool enabled = PropertyUtility.IsEnabled(property);

            var target = PropertyUtility.GetTargetObjectOfProperty(property);
            CurrentTarget = target;

            try
            {
                using (new EditorGUI.DisabledScope(disabled: !enabled))
                {
                    drawProperty(rect, property, PropertyUtility.GetLabel(property));
                }
            }
            finally
            {
                CurrentTarget = null;
            }

            if (EditorGUI.EndChangeCheck())
            {
                PropertyUtility.CallOnValueChangedCallbacks(property);
            }
        }
    }
}
