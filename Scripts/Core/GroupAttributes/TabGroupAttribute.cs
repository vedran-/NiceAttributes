using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEditor;
using UnityEngine;

namespace NiceAttributes
{
    [AttributeUsage( AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Method, AllowMultiple = true )]
    public class TabGroupAttribute : BaseGroupAttribute
    {
        public TabGroupAttribute( string groupName = "", [CallerLineNumber] int lineNumber = 0 ) 
            : base( groupName, lineNumber ) {}

#if UNITY_EDITOR
        public class TabParent
        {
            public List<TabGroupAttribute>  tabGroups = null;
            public string[]                 tabHeader = null;
            public int                      selectedTabIdx = 0;
        }

        public TabParent tabParent = null;
        bool IsSelectedTab => GetSelectedTab() == this;
        Rect tabRect;

        #region GetSelectedTabIdx()
        int GetSelectedTabIdx()
        {
            if( tabParent == null || tabParent.tabGroups == null || tabParent.tabGroups.Count == 0 ) return -1;
            if( tabParent.selectedTabIdx >= tabParent.tabGroups.Count ) tabParent.selectedTabIdx = tabParent.tabGroups.Count - 1;
            if( tabParent.selectedTabIdx < 0 ) tabParent.selectedTabIdx = 0;
            return tabParent.selectedTabIdx;
        }
        #endregion GetSelectedTabIdx()
        #region GetSelectedTab()
        TabGroupAttribute GetSelectedTab()
        {
            var idx = GetSelectedTabIdx();
            return idx >= 0 ? tabParent.tabGroups[idx] : null;
        }
        #endregion GetSelectedTab()


        #region OnGUI_GroupStart()
        private protected override bool OnGUI_GroupStart()
        {
            // Draw tab group only when selected in parent
            if( !IsSelectedTab ) return false;

            // Draw Tab Header
            tabRect = EditorGUILayout.BeginVertical();

            // Fill the background, if set
            if( GroupBackColor != ColorNotSet ) DrawingUtil.FillRect( tabRect, GroupBackColor.ToColor() );

            DrawingUtil.DrawTabHeader( tabParent );

            // Draw client area
            var rect = EditorGUILayout.BeginVertical();

            return true;
        }
        #endregion OnGUI_GroupStart()

        #region OnGUI_GroupEnd()
        private protected override void OnGUI_GroupEnd()
        {
            if( !IsSelectedTab ) return;

            // Calculate number of items drawn by using ControlID - it increases with each control drawn
            //lastItemsDrawn = GUIUtility.GetControlID( FocusType.Passive ) - lastStartId;

            EditorGUILayout.EndVertical();
            GUILayout.Space( 3 );
            EditorGUILayout.EndVertical();

            DrawingUtil.DrawRect( tabRect, Color.black, 2 );
        }
        #endregion OnGUI_GroupEnd()
#endif
    }
}