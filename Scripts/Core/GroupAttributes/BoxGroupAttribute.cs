using System;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace NiceAttributes
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class BoxGroupAttribute : BaseGroupAttribute
    {
        public BoxGroupAttribute( string groupName = "", bool showLabel = true, [CallerLineNumber] int lineNumber = 0 ) 
            : base( groupName, lineNumber )
        {
            ShowLabel = showLabel;
        }

#if UNITY_EDITOR
        Rect drawRect;

        private protected override bool OnGUI_GroupStart()
        {
            drawRect = UnityEditor.EditorGUILayout.BeginVertical( GUI.skin.box );

            // Fill the background, if set
            if( GroupBackColor != ColorNotSet ) DrawingUtil.FillRect( drawRect, GroupBackColor.ToColor() );


            // Show the label
            var label = Title ?? GroupName;
            if( ShowLabel && !string.IsNullOrEmpty( label ) ) DrawingUtil.DrawHeader( label, groupAttr: this );

            return true;
        }

        private protected override void OnGUI_GroupEnd()
        {
            UnityEditor.EditorGUILayout.EndVertical();

            // Draw bounding rect
            DrawingUtil.DrawRect( drawRect, Color.black );
        }
#endif
    }
}
