using UnityEngine;

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
}