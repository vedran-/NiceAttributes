using System;
using System.Runtime.CompilerServices;
#if UNITY_EDITOR
    using UnityEditor;
#endif

namespace NiceAttributes
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class FoldoutAttribute : BaseGroupAttribute
    {
#if UNITY_EDITOR
        private bool _foldedOut = true;
#endif

        //var id = $"{this.target.GetInstanceID()}.{Name}";    // TODO
        string id => GroupName;

        public FoldoutAttribute(string groupName = "", [CallerLineNumber] int lineNumber = 0)
            : base(groupName, lineNumber)
        {
#if UNITY_EDITOR
            _foldedOut = EditorPrefs.GetBool(id, true);
#endif
        }

#if UNITY_EDITOR
        private protected override bool OnGUI_GroupStart()
        {
            var rect = EditorGUILayout.BeginVertical();

            // Fill the background, if set
            if (GroupBackColor.HasValue()) DrawingUtil.FillRect(rect, GroupBackColor.ToColor());

            var label = Title ?? GroupName;
            var folded = EditorGUILayout.Foldout(_foldedOut, GetLabel(), true);
            if (folded != _foldedOut)
            {
                // Value changed
                _foldedOut = folded;
                EditorPrefs.SetBool(id, folded);
            }

            // Return true to draw elements of the group, or false not to draw them
            return _foldedOut;
        }

        private protected override void OnGUI_GroupEnd()
        {
            EditorGUILayout.EndVertical();
        }
#endif
    }
}
