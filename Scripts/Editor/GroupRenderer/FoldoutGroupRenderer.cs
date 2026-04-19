using System.Collections.Generic;
using UnityEditor;

namespace NiceAttributes.Editor.GroupRenderer
{
    public class FoldoutGroupRenderer : BaseGroupRenderer
    {
        private static readonly Dictionary<string, bool> s_FoldoutState = new();
        private bool _foldedOut = true;
        private string _key;

        protected override bool OnGUI_GroupStart(BaseGroupAttribute attr, object target)
        {
            if (_key == null)
            {
                var targetTypeName = target?.GetType().FullName ?? "unknown";
                _key = $"{targetTypeName}__{attr.GroupName}";
            }

            var rect = EditorGUILayout.BeginVertical();

            if (attr.GroupBackColor.HasValue()) GUIUtil.FillRect(rect, attr.GroupBackColor.ToColor());

            var label = attr.Title ?? attr.GroupName;
            if (!s_FoldoutState.ContainsKey(_key))
            {
                s_FoldoutState[_key] = EditorPrefs.GetBool(_key, true);
                _foldedOut = s_FoldoutState[_key];
            }
            var folded = EditorGUILayout.Foldout(_foldedOut, label, true);
            if (folded != _foldedOut)
            {
                _foldedOut = folded;
                s_FoldoutState[_key] = folded;
                EditorPrefs.SetBool(_key, folded);
            }

            return _foldedOut;
        }

        protected override void OnGUI_GroupEnd(BaseGroupAttribute attr, object target)
        {
            EditorGUILayout.EndVertical();
        }
    }
}