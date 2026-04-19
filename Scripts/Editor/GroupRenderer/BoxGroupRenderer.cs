using UnityEditor;
using UnityEngine;

namespace NiceAttributes.Editor.GroupRenderer
{
    public class BoxGroupRenderer : BaseGroupRenderer
    {
        private Rect _drawRect;

        protected override bool OnGUI_GroupStart(BaseGroupAttribute attr, object target)
        {
            _drawRect = EditorGUILayout.BeginVertical(GUI.skin.box);

            if (attr.GroupBackColor.HasValue()) GUIUtil.FillRect(_drawRect, attr.GroupBackColor.ToColor());

            var label = GetLabel(attr);
            if (!string.IsNullOrEmpty(label)) GUIUtil.DrawHeader(label, groupAttr: attr);

            return true;
        }

        protected override void OnGUI_GroupEnd(BaseGroupAttribute attr, object target)
        {
            EditorGUILayout.EndVertical();
            GUIUtil.DrawRect(_drawRect, Color.black);
        }

        private static string GetLabel(BaseGroupAttribute attr)
        {
            if (!string.IsNullOrEmpty(attr.Title)) return attr.Title;
            if (!attr.ShowTitle) return null;
            return attr.GroupName;
        }
    }
}