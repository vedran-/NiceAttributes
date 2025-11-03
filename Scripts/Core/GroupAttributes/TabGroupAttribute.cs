using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEditor;
using UnityEngine;

namespace NiceAttributes
{
    [AttributeUsage( AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Method, AllowMultiple = true )]
    [Conditional("UNITY_EDITOR")]
    public class TabGroupAttribute : BaseGroupAttribute
    {
        public TabGroupAttribute(string groupName = "", [CallerLineNumber] int lineNumber = 0)
            : base(groupName, lineNumber) {}

#if UNITY_EDITOR
        private const float PaddingLeftRight = 8f;
        private const float PaddingTopBottom = 8f;
        private static readonly Color DefaultBackgroundColor = new Color32(53, 52, 68, 255);

        public class TabParent
        {
            public List<TabGroupAttribute>  tabGroups = null;
            public string[]                 tabHeader = null;
            public int                      selectedTabIdx = 0;
        }

        public TabParent tabParent = null;
        private bool IsSelectedTab => GetSelectedTab() == this;

        private int GetSelectedTabIdx()
        {
            if (tabParent == null || tabParent.tabGroups == null || tabParent.tabGroups.Count == 0) return -1;
            if (tabParent.selectedTabIdx >= tabParent.tabGroups.Count) tabParent.selectedTabIdx = tabParent.tabGroups.Count - 1;
            if (tabParent.selectedTabIdx < 0) tabParent.selectedTabIdx = 0;
            return tabParent.selectedTabIdx;
        }

        private TabGroupAttribute GetSelectedTab()
        {
            var idx = GetSelectedTabIdx();
            return idx >= 0 ? tabParent.tabGroups[idx] : null;
        }

        private static Rect DrawTabHeader(TabParent tabParent)
        {
            // Check/create Tab header
            tabParent.tabHeader ??= tabParent.tabGroups.Select(tg => tg.Title ?? tg.GroupName).ToArray();

            GUIUtil.PushColor(Color.white);

#if !USE_OLD_TABGROUP_LOOK
            var fullRect = GUILayoutUtility.GetRect(0, 20);
            var dx = fullRect.width / tabParent.tabGroups.Count;
            GUIUtil.DrawHorizontalLine(fullRect.x, fullRect.yMax - 1, fullRect.width-1, Color.black, 1f);

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
            var newIdx = GUILayout.Toolbar( tabParent.selectedTabIdx, tabParent.tabHeader );
            if( newIdx != tabParent.selectedTabIdx )
            {
                tabParent.selectedTabIdx = newIdx;
                EditorGUIUtility.editingTextField = false;
            }
#endif

            GUIUtil.PopColor();
            return fullRect;
        }


        private protected override bool OnGUI_GroupStart()
        {
            // Draw tab group only when selected in parent
            if (!IsSelectedTab) return false;

            // Draw Tab Header
            EditorGUILayout.BeginVertical();
            GUILayout.Space(4);

            var bgColor = GroupBackColor.HasValue() ? GroupBackColor.ToColor() : DefaultBackgroundColor;
            var headerRect = DrawTabHeader(tabParent);

            // Setup client area
            var clientRect = EditorGUILayout.BeginHorizontal();
            clientRect.width = headerRect.width-1;
            
            GUIUtil.FillRect(clientRect, bgColor, null, GUIStyles.RoundedBottomRect);
            
            GUILayout.Space(PaddingLeftRight);
            EditorGUILayout.BeginVertical();
            GUILayout.Space(PaddingTopBottom);

            return true;
        }

        private protected override void OnGUI_GroupEnd()
        {
            if (!IsSelectedTab) return;

            GUILayout.Space(PaddingTopBottom);
            EditorGUILayout.EndVertical();
            GUILayout.Space(PaddingLeftRight);
            EditorGUILayout.EndHorizontal();
            GUILayout.Space(4);
            EditorGUILayout.EndVertical();
        }
#endif
    }
}