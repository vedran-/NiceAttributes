using System;
using System.Runtime.CompilerServices;

namespace NiceAttributes
{
    [AttributeUsage( AttributeTargets.Field, AllowMultiple = true, Inherited = true )]
    public class HorizontalLineAttribute : DrawerAttribute
    {
        public const float DefaultHeight = 2.0f;
        public const EColor DefaultColor = EColor.Gray;

        public float Height { get; private set; }
        public EColor Color { get; private set; }

        public HorizontalLineAttribute( float height = DefaultHeight, EColor color = DefaultColor, [CallerLineNumber] int lineNumber = 0 )
            : base( lineNumber )
        {
            Height = height;
            Color = color;
        }
    }
}
