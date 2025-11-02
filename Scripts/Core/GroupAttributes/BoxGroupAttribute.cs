using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace NiceAttributes
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    [Conditional("UNITY_EDITOR")]
    public class BoxGroupAttribute : BaseGroupAttribute
    {
        public BoxGroupAttribute( string groupName = "", bool showTitle = true, [CallerLineNumber] int lineNumber = 0 ) 
            : base( groupName, lineNumber )
        {
            ShowTitle = showTitle;
        }

#if UNITY_EDITOR
        private Rect _drawRect;

        private protected override bool OnGUI_GroupStart()
        {
            _drawRect = UnityEditor.EditorGUILayout.BeginVertical( GUI.skin.box );

            // Fill the background, if set
            if( GroupBackColor.HasValue() ) DrawingUtil.FillRect( _drawRect, GroupBackColor.ToColor() );


            // Show the label
            var label = GetLabel();
            if( !string.IsNullOrEmpty( label ) ) DrawingUtil.DrawHeader( label, groupAttr: this );

            return true;
        }

        private protected override void OnGUI_GroupEnd()
        {
            UnityEditor.EditorGUILayout.EndVertical();

            // Draw bounding rect
            DrawingUtil.DrawRect( _drawRect, Color.black );
        }
#endif
    }
}
