using System.Collections.Generic;
using UnityEngine;

namespace NiceAttributes
{
    public static partial class GUIUtil
    {
        private static readonly Color _bgProSkin = new Color32(56, 56, 56, 255);
        private static readonly Color _bgFreeSkin = new Color32(194, 194, 194, 255);

        private static readonly Stack<Color> _bgColorStack = new Stack<Color>();
        private static readonly Stack<Color> _contentColorStack = new Stack<Color>();
        private static readonly Stack<Color> _colorStack = new Stack<Color>();
        
#if UNITY_EDITOR
        public static Color GetDefaultBackgroundColor() => UnityEditor.EditorGUIUtility.isProSkin ? _bgProSkin : _bgFreeSkin;
        //public static Color EditorBackgroundColor = GUI.skin.window.normal.background.GetPixel( 0, 0 );
#endif

        public static void PushBackgroundColor( Color color )
        {
            _bgColorStack.Push( GUI.backgroundColor );
            if (GUI.backgroundColor != color) GUI.backgroundColor = color;
        }
        public static void PopBackgroundColor()
        {
            var color = _bgColorStack.Pop();
            if (GUI.backgroundColor != color) GUI.backgroundColor = color;
        }

        public static void PushContentColor( Color color )
        {
            _contentColorStack.Push( GUI.contentColor );
            if (GUI.contentColor != color) GUI.contentColor = color;
        }
        public static void PopContentColor()
        {
            var color = _contentColorStack.Pop();
            if (GUI.contentColor != color) GUI.contentColor = color;
        }
        
        public static void PushColor( Color color )
        {
            _colorStack.Push( GUI.color );
            if (GUI.color != color) GUI.color = color;
        }
        public static void PopColor()
        {
            var color = _colorStack.Pop();
            if (GUI.color != color) GUI.color = color;
        }
    }
}
