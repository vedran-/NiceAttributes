using System.Linq;
using UnityEditor;
using UnityEngine;

namespace NiceAttributes.Editor.GroupRenderer
{
    public class TabGroupRenderer : BaseGroupRenderer
    {
        private const float PaddingLeftRight = 8f;
        private const float PaddingTopBottom = 8f;
        private static readonly Color DefaultBackgroundColor = new Color32(53, 52, 68, 255);

        protected override bool OnGUI_GroupStart(BaseGroupAttribute attr, object target)
        {
            var tabGroup = attr as TabGroupAttribute;
            if (tabGroup == null) return false;

            if (!tabGroup.IsSelectedTab) return false;

            EditorGUILayout.BeginVertical();
            GUILayout.Space(4);

            var bgColor = attr.GroupBackColor.HasValue() ? attr.GroupBackColor.ToColor() : DefaultBackgroundColor;
            var headerRect = DrawTabHeader(tabGroup.tabParent);

            var clientRect = EditorGUILayout.BeginHorizontal();
            clientRect.width = headerRect.width - 1;

            GUIUtil.FillRect(clientRect, bgColor, null, GUIStyles.RoundedBottomRect);

            GUILayout.Space(PaddingLeftRight);
            EditorGUILayout.BeginVertical();
            GUILayout.Space(PaddingTopBottom);

            return true;
        }

        protected override void OnGUI_GroupEnd(BaseGroupAttribute attr, object target)
        {
            var tabGroup = attr as TabGroupAttribute;
            if (tabGroup == null) return;

            if (!tabGroup.IsSelectedTab) return;

            GUILayout.Space(PaddingTopBottom);
            EditorGUILayout.EndVertical();
            GUILayout.Space(PaddingLeftRight);
            EditorGUILayout.EndHorizontal();
            GUILayout.Space(4);
            EditorGUILayout.EndVertical();
        }

        private static Rect DrawTabHeader(TabGroupAttribute.TabParent tabParent)
        {
            tabParent.tabHeader ??= tabParent.tabGroups.Select(tg => tg.Title ?? tg.GroupName).ToArray();

            GUIUtil.PushColor(Color.white);

#if !USE_OLD_TABGROUP_LOOK
            var fullRect = GUILayoutUtility.GetRect(0, 20);
            var dx = fullRect.width / tabParent.tabGroups.Count;
            GUIUtil.DrawHorizontalLine(fullRect.x, fullRect.yMax - 1, fullRect.width - 1, Color.black, 1f);

            var origBgColor = GUI.backgroundColor;
            var origContentColor = GUI.contentColor;
            for (int idx = 0; idx < tabParent.tabHeader.Length; idx++)
            {
                var rect = fullRect;
                rect.x += dx * idx;
                rect.width = dx;

                var isSelected = idx == tabParent.selectedTabIdx;
                if (!isSelected) rect.height--;

                var bgColor = tabParent.tabGroups[idx].GroupBackColor.HasValue() ? tabParent.tabGroups[idx].GroupBackColor.ToColor() : DefaultBackgroundColor;
                if (!isSelected) bgColor = Color.Lerp(bgColor, Color.black, 0.25f);

                GUI.backgroundColor = bgColor;
                GUI.contentColor = tabParent.tabGroups[idx].TitleColor.HasValue() ? tabParent.tabGroups[idx].TitleColor.ToColor() : Color.white;
                var tabName = isSelected ? $"<u>{tabParent.tabHeader[idx]}</u>" : tabParent.tabHeader[idx];
                if (GUIUtil.InstantClickButton(rect, tabName, GUIStyles.RoundedTopRect))
                {
                    tabParent.selectedTabIdx = idx;
                    if (EditorGUIUtility.editingTextField) EditorGUIUtility.editingTextField = false;
                    Event.current.Use();
                }
                EditorGUIUtility.AddCursorRect(rect, MouseCursor.Link);
            }
            GUI.backgroundColor = origBgColor;
            GUI.contentColor = origContentColor;
#else
            var newIdx = GUILayout.Toolbar(tabParent.selectedTabIdx, tabParent.tabHeader);
            if (newIdx != tabParent.selectedTabIdx)
            {
                tabParent.selectedTabIdx = newIdx;
                EditorGUIUtility.editingTextField = false;
            }
#endif

            GUIUtil.PopColor();
            return fullRect;
        }
    }
}