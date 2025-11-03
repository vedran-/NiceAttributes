using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        private const float PaddingTopBottom = 4f;

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
            if( tabParent == null || tabParent.tabGroups == null || tabParent.tabGroups.Count == 0 ) return -1;
            if( tabParent.selectedTabIdx >= tabParent.tabGroups.Count ) tabParent.selectedTabIdx = tabParent.tabGroups.Count - 1;
            if( tabParent.selectedTabIdx < 0 ) tabParent.selectedTabIdx = 0;
            return tabParent.selectedTabIdx;
        }

        private TabGroupAttribute GetSelectedTab()
        {
            var idx = GetSelectedTabIdx();
            return idx >= 0 ? tabParent.tabGroups[idx] : null;
        }


        private protected override bool OnGUI_GroupStart()
        {
            // Draw tab group only when selected in parent
            if( !IsSelectedTab ) return false;

            // Draw Tab Header
            _tabRect = EditorGUILayout.BeginVertical();

            // Fill the background, if set
            if( GroupBackColor.HasValue() ) DrawingUtil.FillRect( _tabRect, GroupBackColor.ToColor() );

            DrawingUtil.DrawTabHeader( tabParent );

            // Setup client area
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(PaddingLeftRight);
            EditorGUILayout.BeginVertical();
            GUILayout.Space(PaddingTopBottom);

            return true;
        }

        private protected override void OnGUI_GroupEnd()
        {
            if( !IsSelectedTab ) return;

            GUILayout.Space(PaddingTopBottom);
            EditorGUILayout.EndVertical();
            GUILayout.Space(PaddingLeftRight);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();

            DrawingUtil.DrawRect( _tabRect, Color.black, 2 );
        }
#endif
    }
}