using System;
using System.Runtime.CompilerServices;
using UnityEditor;
using UnityEngine;

namespace NiceAttributes
{
    [AttributeUsage( AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Method, AllowMultiple = true )]
    public class HorizontalGroupAttribute : BaseGroupAttribute
    {
        public HorizontalGroupAttribute( string groupName = "", [CallerLineNumber] int lineNumber = 0 ) 
            : base( groupName, lineNumber ) {}

#if UNITY_EDITOR
        int lastItemsDrawn = 0, lastStartId;

        private protected override bool OnGUI_GroupStart()
        {
            var rect = EditorGUILayout.BeginHorizontal();

            // Fill the background, if set
            if( GroupBackColor.HasValue() ) DrawingUtil.FillRect( rect, GroupBackColor.ToColor() );

            lastStartId = GUIUtility.GetControlID( FocusType.Passive );

            // Try to automatically calculate label and field width, so it fits visible space
            if( false && lastItemsDrawn > 0 && (InsideLabelWidth == 0 || InsideFieldWidth == 0) )
            {
                //var w = (rect.width / lastItemsDrawn) / 2;
                var w = (EditorGUIUtility.currentViewWidth / lastItemsDrawn) / 2;
                if( InsideLabelWidth == 0 ) InsideLabelWidth = w;
                if( InsideFieldWidth == 0 ) InsideFieldWidth = w;
                //Debug.Log( $"w: {w:0.#}; sW: {EditorGUIUtility.currentViewWidth:0.#}; rW: {rect.width:0.#}, items: {lastItemsDrawn}" );
            }


            // Show the label
            var label = GetLabel();
            if( !string.IsNullOrEmpty( label ) ) DrawingUtil.DrawHeader( label, true, this );
            return true;
        }

        private protected override void OnGUI_GroupEnd()
        {
            // Calculate number of items drawn by using ControlID - it increases with each control drawn
            lastItemsDrawn = GUIUtility.GetControlID( FocusType.Passive ) - lastStartId;

            EditorGUILayout.EndHorizontal();
        }
#endif
    }
}