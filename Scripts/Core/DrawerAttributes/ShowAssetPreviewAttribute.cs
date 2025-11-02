using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using NiceAttributes.Model;

namespace NiceAttributes
{
    [AttributeUsage( AttributeTargets.Field, AllowMultiple = false, Inherited = true )]
    [Conditional("UNITY_EDITOR")]
    public class ShowAssetPreviewAttribute : DrawerAttribute
    {
        public const int DefaultWidth = 64;
        public const int DefaultHeight = 64;

        public int Width { get; private set; }
        public int Height { get; private set; }

        public ShowAssetPreviewAttribute( int width = DefaultWidth, int height = DefaultHeight, [CallerLineNumber] int lineNumber = 0 )
            : base( lineNumber )
        {
            Width = width;
            Height = height;
        }
    }
}
