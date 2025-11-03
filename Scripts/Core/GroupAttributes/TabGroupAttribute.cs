using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
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
        private Color DefaultBackgroundColor = new Color32(53, 52, 68, 255);

        public class TabParent
        {
            public List<TabGroupAttribute>  tabGroups = null;
            public string[]                 tabHeader = null;
            public int                      selectedTabIdx = 0;
        }

        public TabParent tabParent = null;
        private bool IsSelectedTab => GetSelectedTab() == this;
        private Rect _tabRect;

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

        private static void DrawTabHeader( TabParent tabParent, Color bgColor )
        {
            // Check/create Tab header
            tabParent.tabHeader ??= tabParent.tabGroups.Select(tg => tg.Title ?? tg.GroupName).ToArray();

            var origBgColor = GUI.backgroundColor;
            var origColor = GUI.color;
            GUI.color = Color.white;

            var fullRect = GUILayoutUtility.GetRect(0, 20);
            var dx = fullRect.width / tabParent.tabGroups.Count;
            for (int idx = 0; idx < tabParent.tabHeader.Length; idx++)
            {
                var isSelected = idx == tabParent.selectedTabIdx;
                //var style = isSelected ? GUIStyles.RoundedTopRect : GUIStyles.RoundedRect;
                var rect = fullRect;
                rect.x += dx * idx;
                rect.width = dx;

                GUI.backgroundColor = isSelected ? bgColor : Color.Lerp(bgColor, Color.black, 0.2f);


#if true
                if (GUI.RepeatButton(rect, tabParent.tabHeader[idx], GUIStyles.RoundedTopRect))
#elif true
                if (GUI.Button(rect, tabParent.tabHeader[idx], GUIStyles.RoundedTopRect))
#elif true
                GUI.Button(rect, tabParent.tabHeader[idx], GUIStyles.RoundedTopRect);
                if (Event.current.type == EventType.MouseDown 
                    && Event.current.button == 0
                    && rect.Contains(Event.current.mousePosition))
#endif
                {
                    tabParent.selectedTabIdx = idx;
                    if (EditorGUIUtility.editingTextField) EditorGUIUtility.editingTextField = false;
                }
            }

            // var newIdx = GUILayout.Toolbar( tabParent.selectedTabIdx, tabParent.tabHeader );
            // if( newIdx != tabParent.selectedTabIdx )
            // {
            //     tabParent.selectedTabIdx = newIdx;
            //     EditorGUIUtility.editingTextField = false;
            // }
            
            GUI.backgroundColor = origBgColor;
            GUI.color = origColor;
        }


        private protected override bool OnGUI_GroupStart()
        {
            // Draw tab group only when selected in parent
            if (!IsSelectedTab) return false;

            // Draw Tab Header
            _tabRect = EditorGUILayout.BeginVertical();
            GUILayout.Space(4);

            var bgColor = GroupBackColor.HasValue() ? GroupBackColor.ToColor() 
                : DefaultBackgroundColor;

            DrawTabHeader(tabParent, bgColor);

            // Setup client area
            var clientRect = EditorGUILayout.BeginHorizontal();
            
            DrawingUtil.FillRect(clientRect, bgColor, null, GUIStyles.RoundedBottomRect);
            
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