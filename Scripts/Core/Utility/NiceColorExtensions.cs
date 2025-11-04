#if UNITY_EDITOR
using UnityEngine;

namespace NiceAttributes
{
    public static class NiceColorExtensions
    {
        public static Color ToColor( this NiceColor color )
        {
            var val = (uint)color;
            return new Color32( 
                (byte)((val >> 24) & 0xFF), 
                (byte)((val >> 16) & 0xFF), 
                (byte)((val >> 8) & 0xFF), 
                (byte)((val >> 0) & 0xFF) );
        }

        public static bool HasValue(this NiceColor color) => color != NiceColor.ColorNotSet;
        
        public static NiceColor ToEColor(this int value) => (NiceColor)value;
        public static NiceColor ToEColor(this uint value) => (NiceColor)value;
    }
}
#endif