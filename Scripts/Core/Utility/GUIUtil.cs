#if UNITY_EDITOR
using System;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace NiceAttributes
{
    public static partial class GUIUtil
    {
        private static readonly Color _headerBgColor = new Color32( 3, 45, 53, 255 );

        public static void DrawHorizontalLine( float x, float y, float width, Color color, float thickness = 1 ) 
            => FillRect( new Rect( x, y - thickness/2f, width, thickness ), color );
        public static void DrawVerticalLine( float x, float y, float height, Color color, float thickness = 1 ) 
            => FillRect( new Rect( x - thickness/2f, y, thickness, height ), color );

        public static void DrawRect( Rect rect, Color color, float thickness = 1f )
        {
            DrawHorizontalLine( rect.x, rect.yMin, rect.width, color, thickness );
            DrawHorizontalLine( rect.x, rect.yMax, rect.width, color, thickness );
            DrawVerticalLine( rect.xMin, rect.y, rect.height, color, thickness );
            DrawVerticalLine( rect.xMax, rect.y, rect.height, color, thickness );
        }

        public static void FillRect(Rect rect, Color color, GUIContent content = null, GUIStyle style = null)
        {
            if (style == null)
            {
                EditorGUI.DrawRect(rect, color);
            }
            else
            {
                PushBackgroundColor(color);
                GUI.Box(rect, content ?? GUIContent.none, style);
                PopBackgroundColor();
            }
        }

        public static void FillRoundedRect(Rect rect, Color color, GUIContent content = null) =>
            FillRect(rect, color, content, GUIStyles.RoundedRect);
        public static void FillRoundedTopRect(Rect rect, Color color, GUIContent content = null) =>
            FillRect(rect, color, content, GUIStyles.RoundedTopRect);

        public static bool InstantClickButton(Rect rect, string text, GUIStyle style = null)
        {
#if false
            return GUI.Button(rect, text, style ?? GUI.skin.button);
#elif true
            GUI.Box(rect, text, style ?? GUI.skin.button);
            var e = Event.current;
            if (e.type == EventType.MouseDown && e.button == 0 && rect.Contains(e.mousePosition))
            {
                e.Use();
                return true;
            }
            return false;
#else
            GUI.Box(rect, text, style ?? GUI.skin.button);
            int id = GUIUtility.GetControlID(FocusType.Passive);
            if (Event.current.GetTypeForControl(id) == EventType.MouseDown
                && Event.current.button == 0
                && rect.Contains(Event.current.mousePosition))
            {
                e.Use();
                return true;
            }
            return false;
#endif
        }
        

        /// <summary>
        /// Draws a label with optional shadow and editable text field.
        /// </summary>
        /// <param name="rect">Drawing rectangle.</param>
        /// <param name="text">Text to display (or edit).</param>
        /// <param name="guiStyle"></param>
        /// <param name="textColor"></param>
        /// <param name="shadowColor"></param>
        /// <param name="setCellValue">If set, then we'll allow label text to be edited, and this method will be called when text value changes.</param>
        public static void DrawLabel( Rect rect, string text, GUIStyle guiStyle, 
            Color? textColor = null, Color? shadowColor = null, 
            Action<string> setCellValue = null )
        {
            var origContentColor = GUI.contentColor;
            var textCol = textColor ?? GUI.contentColor;

            // If we can set value of the cell - show TextField so user can edit the value
            if( setCellValue != null )
            {
                PushColor(textCol);
                var newText = GUI.TextField( rect, text, guiStyle );
                if( newText != text ) setCellValue( newText );
                GUI.contentColor = origContentColor;
                PopColor();

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
                GUI.Label( rect, guiStyle.richText ? RemoveColorRichTextTags(text) : text, guiStyle );
                // Draw text
                GUI.contentColor = textCol;
                GUI.Label( rTxt, text, guiStyle );
            } else {
                // Draw normal text
                GUI.contentColor = textCol;
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

        public static GUIStyle GUIStyleFromSplicedSprite(Sprite sprite)
        {
            var style = new GUIStyle();
            style.clipping = TextClipping.Clip;
            style.alignment = TextAnchor.MiddleCenter;
            style.imagePosition = ImagePosition.ImageLeft;
            style.normal.background = sprite.texture;
            style.normal.textColor = Color.white;
            style.border = new RectOffset(
                Mathf.RoundToInt(sprite.border.x), 
                Mathf.RoundToInt(sprite.border.z), 
                Mathf.RoundToInt(sprite.border.w), 
                Mathf.RoundToInt(sprite.border.y)
            );
            style.stretchWidth = true;
            style.stretchHeight = true;
            style.richText = true;
            style.padding = new RectOffset(4, 4, 2, 2);
            //style.margin = new RectOffset(20, 0, 8, 0);
            //style.overflow = new RectOffset(0, 0, -5, -5);
            return style;
        }
        

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

        public static void DrawHeader(string label, bool fitWidth = false, BaseGroupAttribute groupAttr = null)
        {
            var size = EditorStyles.boldLabel.CalcSize(new GUIContent(label));

            var rect = fitWidth
                ? EditorGUILayout.GetControlRect(GUILayout.MaxWidth(size.x), GUILayout.MaxHeight(size.y))
                : EditorGUILayout.GetControlRect(GUILayout.MaxHeight(size.y));
            var bgRect = fitWidth ? rect.Grow(3, 3, 3, 3) : rect.Grow(3, 3, 3, 0);
            var bgCol = groupAttr == null || !groupAttr.TitleBackColor.HasValue()
                ? _headerBgColor
                : groupAttr.TitleBackColor.ToColor();
            FillRoundedRect(bgRect, bgCol);

            if (!fitWidth) rect.y -= 2;
            var fgCol = groupAttr == null || !groupAttr.TitleColor.HasValue() ? Color.white : groupAttr.TitleColor.ToColor();
            var shadowCol = groupAttr == null || !groupAttr.TitleShadowColor.HasValue() ? Color.gray : groupAttr.TitleShadowColor.ToColor();
            DrawLabel(rect, label, EditorStyles.boldLabel, fgCol, shadowCol);
        }
    }
}
#endif
