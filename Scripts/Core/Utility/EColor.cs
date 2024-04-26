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

#if UNITY_EDITOR
    public static class EColorExtensions
    {
    }
#endif
}
