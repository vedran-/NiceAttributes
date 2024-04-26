#if UNITY_EDITOR

using System;
using System.Text.RegularExpressions;
using UnityEngine;
using System.Linq;
#if UNITY_EDITOR
    using UnityEditor;
#endif

namespace NiceAttributes
{
    public static class Util
    {
        #region [EColor] ToColor()
        public static Color ToColor( this EColor color )
        {
            var val = (uint)color;
            return new Color32( (byte)((val >> 24) & 0xFF), (byte)((val >> 16) & 0xFF), (byte)((val >> 8) & 0xFF), (byte)((val >> 0) & 0xFF) );
        }
        #endregion ToColor()

        #region [Color] WithAlpha()
        public static Color WithAlpha( this Color color, float alpha )
        {
            color.a = alpha;
            return color;
        }
        #endregion WithAlpha()

        #region [Rect] Grow()
        public static Rect Grow( this Rect rect, float left, float right, float top, float bottom )
        {
            rect.x -= left;
            rect.y -= top;
            rect.width += left + right;
            rect.height += top + bottom;
            return rect;
        }
        #endregion Grow()
    }

    public static class DrawingUtil
    {
        #region GetDefaultBackgroundColor()
#if UNITY_EDITOR
        static readonly Color BgProSkin = new Color32(56, 56, 56, 255), BgPoorSkin = new Color32(194, 194, 194, 255);
        public static Color GetDefaultBackgroundColor() => EditorGUIUtility.isProSkin ? BgProSkin : BgPoorSkin;
        //static Color bgColorToRed = Color.Lerp( GUI.skin.window.normal.background.GetPixel( 0, 0 ), Color.red, 0.10f );
#endif
        #endregion GetDefaultBackgroundColor()

        #region [Line] DrawHorizontalLine()
        public static void DrawHorizontalLine( float x, float y, float width, Color color, float thickness = 1 ) => FillRect( new Rect( x, y-thickness/2f, width, thickness ), color );
        #endregion DrawHorizontalLine()
        #region [Line] DrawVerticalLine()
        public static void DrawVerticalLine( float x, float y, float height, Color color, float thickness = 1 ) => FillRect( new Rect( x - thickness / 2f, y, thickness, height ), color );
        #endregion DrawVerticalLine()

        #region [Rect] DrawRect()
        public static void DrawRect( Rect rect, Color color, float thickness = 1f )
        {
            DrawHorizontalLine( rect.x, rect.yMin, rect.width, color, thickness );
            DrawHorizontalLine( rect.x, rect.yMax, rect.width, color, thickness );
            DrawVerticalLine( rect.xMin, rect.y, rect.height, color, thickness );
            DrawVerticalLine( rect.xMax, rect.y, rect.height, color, thickness );
        }
        #endregion DrawRect()
        #region [Rect] FillRect()
        private static Texture2D drawRectBackgroundTexture;
        private static GUIStyle drawRectTextureStyle;
        public static void FillRect( Rect rect, Color color, GUIContent content = null )
        {
            if( !drawRectBackgroundTexture ) drawRectBackgroundTexture = Texture2D.whiteTexture;
            if( drawRectTextureStyle == null ) drawRectTextureStyle = new GUIStyle { normal = new GUIStyleState { background = drawRectBackgroundTexture } };

            var backgroundColor = GUI.backgroundColor;
            GUI.backgroundColor = color;
            GUI.Box( rect, content ?? GUIContent.none, drawRectTextureStyle );
            GUI.backgroundColor = backgroundColor;
        }
        #endregion FillRect()



        #region [Label] DrawLabel() - draws with shadow too
        public static void DrawLabel( Rect rect, string text, GUIStyle guiStyle, Color textColor, Color? shadowColor, Action<string> setCellValue = null )
        {
            var origContentColor = GUI.contentColor;

            // If we can set value of the cell - show TextField so user can edit the value
            if( setCellValue != null )
            {
                var origColor = GUI.color;
                GUI.color = textColor;                
                var newText = GUI.TextField( rect, text, guiStyle );
                if( newText != text ) setCellValue( newText );
                GUI.contentColor = origContentColor;
                GUI.color = origColor;

                // Draw small gray rectangle, so user will know that this field is editable
                rect.xMin--; rect.yMin--; rect.xMax++; rect.yMax++;
                //rect.width = rect.height = 2;
                DrawRect( rect, Color.gray );
                return;
            }

            if( shadowColor.HasValue )
            {
                // Draw shadow
                var rTxt = rect;
                rect.x += 1f; rect.y += 0.5f;
                GUI.contentColor = shadowColor.Value;
                GUI.Label( rect, RemoveColorRichTextTags(text), guiStyle );
                // Draw text
                GUI.contentColor = textColor;
                GUI.Label( rTxt, text, guiStyle );
            } else {
                // Draw normal text
                GUI.contentColor = textColor;
                GUI.Label( rect, text, guiStyle );
            }

            GUI.contentColor = origContentColor;
        }
        #endregion DrawLabel()

        #region [Window GUI] GetRectToFillWindow()
        /// <summary>
        /// Gets size of Rect, which would fill all available window space, to bottom of window.
        /// It uses a trick - ScrollView always fills window
        /// </summary>
        /// <param name="minHeight"></param>
        /// <returns></returns>
        public static Rect GetRectToFillWindow( float minHeight )
        {
            var wasEnabled = GUI.enabled;
            GUI.enabled = false;
            var lastRect = GUILayoutUtility.GetLastRect();
            var startPosY = lastRect.yMax;
            var scrollPosition = Vector2.zero;

            // Create ScrollView
            GUILayout.BeginScrollView( scrollPosition, GUIStyle.none, GUIStyle.none );
            // Fill scroller to max height
            GUILayoutUtility.GetRect( lastRect.width, minHeight );
            GUILayout.EndScrollView();

            // Now get rect with height=0, and we'll use its position to know where is bottom of the screen
            var rect = GUILayoutUtility.GetRect( lastRect.width, 0 );
            rect.yMin = startPosY;
            GUI.enabled = wasEnabled;
            return rect;
        }
        #endregion GetRectToFillWindow()

        private static string RemoveColorRichTextTags( string str ) => Regex.Replace( str, @"\<(\/)?color(=""?#?\w+""?)?\>", "" );



        #region [Rect-Editor only] DrawCheckeredRect()
#if UNITY_EDITOR
        public static void DrawCheckeredRect( Rect rect, float xSize = 10, float ySize = 10, Color? color = null, float colorFactorTowardsWhite = 0.5f )
        {
            Color col1 = color ?? Color.gray;
            Color col2 = Color.Lerp( col1, Color.white, colorFactorTowardsWhite ).WithAlpha( col1.a );
            EditorGUI.DrawRect( rect, col1 );

            for( int y = 0; y < rect.height / ySize; y++ )
            {
                for( int x = 0; x < rect.width / xSize; x++ )
                {
                    if( ((x ^ y) & 1) != 0 ) continue;
                    var r = new Rect( rect.xMin + x * xSize, rect.yMin + y * ySize, xSize, ySize );
                    if( r.xMax > rect.xMax ) r.xMax = rect.xMax;
                    if( r.yMax > rect.yMax ) r.yMax = rect.yMax;
                    EditorGUI.DrawRect( r, col2 );
                }
            }
        }
#endif
        #endregion DrawCheckeredRect()

        #region [Editor only] DrawHeader()
        static readonly Color HeaderBgColor = new Color32( 3, 45, 53, 255 );
        public static void DrawHeader( string label, bool fitWidth = false, BaseGroupAttribute groupAttr = null )
        {
            var size = EditorStyles.boldLabel.CalcSize( new GUIContent( label ) );

            var rect = fitWidth 
                ? EditorGUILayout.GetControlRect( GUILayout.MaxWidth( size.x ), GUILayout.MaxHeight( size.y ) )
                : EditorGUILayout.GetControlRect( GUILayout.MaxHeight( size.y ) );
            var bgRect = fitWidth ? rect.Grow( 3, 3, 3, 3 ) : rect.Grow( 3, 3, 3, 0 );
            var bgCol = groupAttr == null || groupAttr.TitleBackColor == BaseGroupAttribute.ColorNotSet ? HeaderBgColor : groupAttr.TitleBackColor.ToColor();
            FillRect( bgRect, bgCol );

            if( !fitWidth ) rect.y -= 2;
            var fgCol = groupAttr == null || groupAttr.TitleColor == BaseGroupAttribute.ColorNotSet ? Color.white : groupAttr.TitleColor.ToColor();
            var shadowCol = groupAttr == null || groupAttr.TitleShadowColor == BaseGroupAttribute.ColorNotSet ? Color.gray : groupAttr.TitleShadowColor.ToColor();
            DrawLabel( rect, label, EditorStyles.boldLabel, fgCol, shadowCol );
        }
        #endregion DrawHeader()

        #region DrawTabHeader()
        public static void DrawTabHeader( TabGroupAttribute.TabParent tabParent )
        {
            // Check/create Tab header
            tabParent.tabHeader ??= tabParent.tabGroups.Select(tg => tg.Title ?? tg.GroupName).ToArray();

            var newIdx = GUILayout.Toolbar( tabParent.selectedTabIdx, tabParent.tabHeader );
            if( newIdx != tabParent.selectedTabIdx )
            {
                tabParent.selectedTabIdx = newIdx;
                EditorGUIUtility.editingTextField = false;
            }
        }
        #endregion DrawTabHeader()
    }
}
#endif
