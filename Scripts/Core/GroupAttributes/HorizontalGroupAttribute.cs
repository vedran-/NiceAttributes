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

        public override bool OnGUI_GroupStart( string label )
        {
            var rect = EditorGUILayout.BeginHorizontal();

            // Fill the background, if set
            if( BackColor != DefaultColor ) DrawingUtil.FillRect( rect, BackColor.ToColor() );

            lastStartId = GUIUtility.GetControlID( FocusType.Passive );

            // Try to automatically calculate label and field width, so it fits visible space
            if( false && lastItemsDrawn > 0 && (LabelWidth == 0 || FieldWidth == 0) )
            {
                //var w = (rect.width / lastItemsDrawn) / 2;
                var w = (EditorGUIUtility.currentViewWidth / lastItemsDrawn) / 2;
                if( LabelWidth == 0 ) LabelWidth = w;
                if( FieldWidth == 0 ) FieldWidth = w;
                //Debug.Log( $"w: {w:0.#}; sW: {EditorGUIUtility.currentViewWidth:0.#}; rW: {rect.width:0.#}, items: {lastItemsDrawn}" );
            }

            SetLabelAndFieldWidth();


            // Show the label
            if( ShowLabel && !string.IsNullOrEmpty( label ) ) DrawingUtil.DrawHeader( label, true, this );
            return true;
        }

        public override void OnGUI_GroupEnd()
        {
            // Calculate number of items drawn by using ControlID - it increases with each control drawn
            lastItemsDrawn = GUIUtility.GetControlID( FocusType.Passive ) - lastStartId;

            RestoreLabelAndFieldWidth();
            EditorGUILayout.EndHorizontal();
        }
#endif
    }
}