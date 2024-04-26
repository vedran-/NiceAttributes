using System;
using System.Runtime.CompilerServices;
#if UNITY_EDITOR
    using UnityEditor;
#endif

namespace NiceAttributes
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class FoldoutAttribute : BaseGroupAttribute
    {
#if UNITY_EDITOR
        bool foldedOut = true;
#endif

        //var id = $"{this.target.GetInstanceID()}.{Name}";    // TODO
        string id => GroupName;

        public FoldoutAttribute( string groupName = "", [CallerLineNumber] int lineNumber = 0 )
            : base( groupName, lineNumber )
        {
#if UNITY_EDITOR
            foldedOut = EditorPrefs.GetBool( id, true );
#endif
        }

#if UNITY_EDITOR
        public override bool OnGUI_GroupStart()
        {
            var rect = EditorGUILayout.BeginVertical();
            SetLabelAndFieldWidth();

            // Fill the background, if set
            if( BackColor != ColorNotSet ) DrawingUtil.FillRect( rect, BackColor.ToColor() );

            var label = Label ?? GroupName;
            var folded = EditorGUILayout.Foldout( foldedOut, ShowLabel ? label : "", true );
            if( folded != foldedOut ) { // Value changed
                foldedOut = folded;
                EditorPrefs.SetBool( id, folded );
            }

            // Return true to draw elements of the group, or false not to draw them
            return foldedOut;
        }

        public override void OnGUI_GroupEnd()
        {
            RestoreLabelAndFieldWidth();
            EditorGUILayout.EndVertical();
        }
#endif
    }
}
