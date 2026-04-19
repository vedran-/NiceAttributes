using UnityEditor;

namespace NiceAttributes.Editor.GroupRenderer
{
    public class VerticalGroupRenderer : BaseGroupRenderer
    {
        protected override bool OnGUI_GroupStart(BaseGroupAttribute attr, object target)
        {
            var rect = EditorGUILayout.BeginVertical();

            if (attr.GroupBackColor.HasValue()) GUIUtil.FillRect(rect, attr.GroupBackColor.ToColor());

            var label = GetLabel(attr);
            if (!string.IsNullOrEmpty(label)) GUIUtil.DrawHeader(label, groupAttr: attr);
            return true;
        }

        protected override void OnGUI_GroupEnd(BaseGroupAttribute attr, object target)
        {
            EditorGUILayout.EndVertical();
        }

        private static string GetLabel(BaseGroupAttribute attr)
        {
            if (!string.IsNullOrEmpty(attr.Title)) return attr.Title;
            if (!attr.ShowTitle) return null;
            return attr.GroupName;
        }
    }
}