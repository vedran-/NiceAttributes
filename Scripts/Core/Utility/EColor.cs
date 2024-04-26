using UnityEngine;

namespace NiceAttributes
{
    /// <summary>
    /// Enum value is actually an RGBA color, in format 0xRRGGBBAA
    /// </summary>
    public enum EColor : uint
    {
        Clear   = 0,
        White   = 0xffffffff,
        Black   = 0x000000ff,
        Gray    = 0x7f7f7fff,
        Red     = 0xff3f00ff,
        Pink    = 0xff98cbff,
        Orange  = 0xff7f00ff,
        Yellow  = 0xffd300ff,
        Green   = 0x62c84fff,
        Blue    = 0x0078bdff,
        Indigo  = 0x4b0082ff,
        Violet  = 0x8000ffff
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
