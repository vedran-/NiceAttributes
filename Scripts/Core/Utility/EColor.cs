using UnityEngine;

namespace NiceAttributes
{
    /// <summary>
    /// Enum value is actually a RGBA color, in format 0xAABBGGRR
    /// </summary>
    public enum EColor : long
    {
        Clear   = 0,
        White   = 0xffffffff,
        Black   = 0xff000000,
        Gray    = 0xff7f7f7f,
        Red     = 0xff003fff,
        Pink    = 0xffcb98ff,
        Orange  = 0xff007fff,
        Yellow  = 0xff00d3ff,
        Green   = 0xff4fc862,
        Blue    = 0xffBD8700,
        Indigo  = 0xff82004B,
        Violet  = 0xffff0080
    }

    public static class EColorExtensions
    {
        public static Color GetColor( this EColor color )
        {
            var c = (uint)color;
            return new Color32( (byte)((c>>0) & 0xff), (byte)((c>>8) & 0xff), (byte)((c>>16) & 0xff), (byte)((c>>24) & 0xff) );
        }
    }
}
