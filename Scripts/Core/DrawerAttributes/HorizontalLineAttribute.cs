using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using NiceAttributes.Model;

namespace NiceAttributes
{
    [AttributeUsage( AttributeTargets.Field, AllowMultiple = true, Inherited = true )]
    [Conditional("UNITY_EDITOR")]
    public class HorizontalLineAttribute : DrawerAttribute
    {
        public const float DefaultHeight = 2.0f;
        public const NiceColor DefaultColor = NiceColor.Gray;

        public float Height { get; private set; }
        public NiceColor Color { get; private set; }

        public HorizontalLineAttribute( float height = DefaultHeight, NiceColor color = DefaultColor, [CallerLineNumber] int lineNumber = 0 )
            : base( lineNumber )
        {
            Height = height;
            Color = color;
        }
    }
}
