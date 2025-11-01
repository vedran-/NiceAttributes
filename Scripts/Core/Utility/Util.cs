using UnityEngine;

namespace NiceAttributes
{
    public static class Util
    {
        #region [Color] WithAlpha()
        public static Color WithAlpha( this Color color, float alpha )
        {
            color.a = alpha;
            return color;
        }
        #endregion WithAlpha()

        public static Color WithMultipliedAlpha( this Color color, float alpha )
        {
            color.a *= alpha;
            return color;
        }

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