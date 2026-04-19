using UnityEditor;
using UnityEngine;

namespace NiceAttributes.Editor.GroupRenderer
{
    public class HorizontalGroupRenderer : BaseGroupRenderer
    {
        private int lastItemsDrawn = 0, lastStartId;

        protected override bool OnGUI_GroupStart(BaseGroupAttribute attr, object target)
        {
            var rect = EditorGUILayout.BeginHorizontal();

            if (attr.GroupBackColor.HasValue()) GUIUtil.FillRect(rect, attr.GroupBackColor.ToColor());

            lastStartId = GUIUtility.GetControlID(FocusType.Passive);

            var label = GetLabel(attr);
            if (!string.IsNullOrEmpty(label)) GUIUtil.DrawHeader(label, true, attr);
            return true;
        }

        protected override void OnGUI_GroupEnd(BaseGroupAttribute attr, object target)
        {
            lastItemsDrawn = GUIUtility.GetControlID(FocusType.Passive) - lastStartId;
            EditorGUILayout.EndHorizontal();
        }

        private static string GetLabel(BaseGroupAttribute attr)
        {
            if (!string.IsNullOrEmpty(attr.Title)) return attr.Title;
            if (!attr.ShowTitle) return null;
            return attr.GroupName;
        }
    }
}