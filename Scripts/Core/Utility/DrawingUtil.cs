#if UNITY_EDITOR
using System;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace NiceAttributes
{
    public static class DrawingUtil
    {
        private static readonly Color _bgProSkin = new Color32(56, 56, 56, 255),
            _bgPoorSkin = new Color32(194, 194, 194, 255);
        private static Texture2D _fillRectBackgroundTexture;
        private static GUIStyle _fillRectTextureStyle;
        private static readonly Color _headerBgColor = new Color32( 3, 45, 53, 255 );
        
        
        public static Color GetDefaultBackgroundColor() => EditorGUIUtility.isProSkin ? _bgProSkin : _bgPoorSkin;
        //public static Color bgColorToRed = Color.Lerp( GUI.skin.window.normal.background.GetPixel( 0, 0 ), Color.red, 0.10f );

        public static void DrawHorizontalLine( float x, float y, float width, Color color, float thickness = 1 ) => FillRect( new Rect( x, y-thickness/2f, width, thickness ), color );
        public static void DrawVerticalLine( float x, float y, float height, Color color, float thickness = 1 ) => FillRect( new Rect( x - thickness / 2f, y, thickness, height ), color );

        public static void DrawRect( Rect rect, Color color, float thickness = 1f )
        {
            DrawHorizontalLine( rect.x, rect.yMin, rect.width, color, thickness );
            DrawHorizontalLine( rect.x, rect.yMax, rect.width, color, thickness );
            DrawVerticalLine( rect.xMin, rect.y, rect.height, color, thickness );
            DrawVerticalLine( rect.xMax, rect.y, rect.height, color, thickness );
        }

        public static void FillRect( Rect rect, Color color, GUIContent content = null )
        {
            if( !_fillRectBackgroundTexture ) _fillRectBackgroundTexture = Texture2D.whiteTexture;
            if( _fillRectTextureStyle == null ) _fillRectTextureStyle = new GUIStyle { normal = new GUIStyleState { background = _fillRectBackgroundTexture } };

            var backgroundColor = GUI.backgroundColor;
            GUI.backgroundColor = color;
            GUI.Box( rect, content ?? GUIContent.none, _fillRectTextureStyle );
            GUI.backgroundColor = backgroundColor;
        }


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

        private static string RemoveColorRichTextTags( string str ) => Regex.Replace( str, @"\<(\/)?color(=""?#?\w+""?)?\>", "" );



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

        public static void DrawHeader( string label, bool fitWidth = false, BaseGroupAttribute groupAttr = null )
        {
            var size = EditorStyles.boldLabel.CalcSize( new GUIContent( label ) );

            var rect = fitWidth 
                ? EditorGUILayout.GetControlRect( GUILayout.MaxWidth( size.x ), GUILayout.MaxHeight( size.y ) )
                : EditorGUILayout.GetControlRect( GUILayout.MaxHeight( size.y ) );
            var bgRect = fitWidth ? rect.Grow( 3, 3, 3, 3 ) : rect.Grow( 3, 3, 3, 0 );
            var bgCol = groupAttr == null || !groupAttr.TitleBackColor.HasValue() ? _headerBgColor : groupAttr.TitleBackColor.ToColor();
            FillRect( bgRect, bgCol );

            if( !fitWidth ) rect.y -= 2;
            var fgCol = groupAttr == null || !groupAttr.TitleColor.HasValue() ? Color.white : groupAttr.TitleColor.ToColor();
            var shadowCol = groupAttr == null || !groupAttr.TitleShadowColor.HasValue() ? Color.gray : groupAttr.TitleShadowColor.ToColor();
            DrawLabel( rect, label, EditorStyles.boldLabel, fgCol, shadowCol );
        }

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
    }
}
#endif
