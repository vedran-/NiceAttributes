using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEditor;

namespace NiceAttributes
{
    [AttributeUsage( AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Method, AllowMultiple = true )]
    [Conditional("UNITY_EDITOR")]
    public class VerticalGroupAttribute : BaseGroupAttribute
    {
        public VerticalGroupAttribute( string groupName = "", [CallerLineNumber] int lineNumber = 0 ) 
            : base( groupName, lineNumber ) {}

#if UNITY_EDITOR
        private protected override bool OnGUI_GroupStart()
        {
            var rect = EditorGUILayout.BeginVertical();

            // Fill the background, if set
            if( GroupBackColor.HasValue() ) DrawingUtil.FillRect( rect, GroupBackColor.ToColor() );

            var label = GetLabel();
            if( !string.IsNullOrEmpty( label ) ) DrawingUtil.DrawHeader( label, groupAttr: this );
            return true;
        }

        private protected override void OnGUI_GroupEnd()
        {
            EditorGUILayout.EndVertical();
        }
#endif
    }
}