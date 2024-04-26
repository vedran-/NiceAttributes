using System;
using System.Runtime.CompilerServices;
using UnityEditor;

namespace NiceAttributes
{
    [AttributeUsage( AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Method, AllowMultiple = true )]
    public class VerticalGroupAttribute : BaseGroupAttribute
    {
        public VerticalGroupAttribute( string groupName = "", [CallerLineNumber] int lineNumber = 0 ) 
            : base( groupName, lineNumber ) {}

#if UNITY_EDITOR
        public override bool OnGUI_GroupStart()
        {
            var rect = EditorGUILayout.BeginVertical();
            SetLabelAndFieldWidth();

            // Fill the background, if set
            if( BackColor != ColorNotSet ) DrawingUtil.FillRect( rect, BackColor.ToColor() );

            var label = Label ?? GroupName;
            if( ShowLabel && !string.IsNullOrEmpty( label ) ) DrawingUtil.DrawHeader( label, groupAttr: this );
            return true;
        }

        public override void OnGUI_GroupEnd()
        {
            RestoreLabelAndFieldWidth();
            EditorGUILayout.EndVertical();
        }
#endif
    }
}