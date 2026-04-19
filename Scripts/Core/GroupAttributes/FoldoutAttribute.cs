#if UNITY_EDITOR
using NiceAttributes.Editor.Utility;
#endif

using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEditor;

namespace NiceAttributes
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    [Conditional("UNITY_EDITOR")]
    public class FoldoutAttribute : BaseGroupAttribute
    {
        private const string _prefsPrefix = "NiceAttributes_Foldout_";

        public FoldoutAttribute(string groupName = "", [CallerLineNumber] int lineNumber = 0)
            : base(groupName, lineNumber)
        {
        }

#if UNITY_EDITOR
        private string GetFoldoutKey()
        {
            var targetId = PropertyDrawPipeline.CurrentTarget?.GetInstanceID() ?? 0;
            return _prefsPrefix + targetId + "_" + GroupName;
        }

        private protected override bool OnGUI_GroupStart()
        {
            var rect = EditorGUILayout.BeginVertical();
            if (GroupBackColor.HasValue()) GUIUtil.FillRect(rect, GroupBackColor.ToColor());
            var label = Title ?? GroupName;
            var key = GetFoldoutKey();
            var folded = EditorPrefs.GetBool(key, true);
            var foldedOut = EditorGUILayout.Foldout(folded, GetLabel(), true);
            if (foldedOut != folded)
            {
                EditorPrefs.SetBool(key, foldedOut);
            }
            return foldedOut;
        }

        private protected override void OnGUI_GroupEnd()
        {
            EditorGUILayout.EndVertical();
        }
#endif
    }
}