using System;
using NiceAttributes.Editor.PropertyValidators;
using NiceAttributes.Model;
using UnityEditor;
using UnityEngine;

namespace NiceAttributes.Editor.Utility
{
    public static class PropertyDrawPipeline
    {
        /// <summary>
        /// The target object of the currently-drawing property.
        /// Set by PropertyDrawPipeline.Execute() and by StartDrawingGroup(object target).
        /// NOTE: This is a static field — it reflects the target of the most recently
        /// drawn property/group in the current editor drawing pass. Since Unity's editor
        /// is single-threaded and processes one GUI pass at a time, this is safe for
        /// normal usage. However, group attributes (like FoldoutAttribute) read this
        /// BEFORE the property pipeline sets it, so they should use SetTarget() to
        /// pass the correct target explicitly via StartDrawingGroup(target).
        /// </summary>
        public static object CurrentTarget { get; internal set; }

        /// <summary>
        /// Sets the current target object for group attributes that need it
        /// during their OnGUI_GroupStart() calls.
        /// </summary>
        internal static void SetTarget(object target) => CurrentTarget = target;

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
