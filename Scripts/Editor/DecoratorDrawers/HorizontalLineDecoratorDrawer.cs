using NiceAttributes.Editor.Utility;
using UnityEditor;
using UnityEngine;

namespace NiceAttributes.Editor.DecoratorDrawers
{
    [CustomPropertyDrawer( typeof( HorizontalLineAttribute ) )]
    public class HorizontalLineDecoratorDrawer : DecoratorDrawer
    {
        public override float GetHeight()
        {
            var lineAttr = (HorizontalLineAttribute)attribute;
            return EditorGUIUtility.singleLineHeight + lineAttr.Height;
        }

        public override void OnGUI( Rect position )
        {
            Rect rect = EditorGUI.IndentedRect(position);
            rect.y += EditorGUIUtility.singleLineHeight / 3.0f;
            var lineAttr = (HorizontalLineAttribute)attribute;
            NiceEditorGUI.HorizontalLine( rect, lineAttr.Height, lineAttr.Color.ToColor() );
        }
    }
}
