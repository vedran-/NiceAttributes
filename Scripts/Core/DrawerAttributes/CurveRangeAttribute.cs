using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using NiceAttributes.Model;
using UnityEngine;

namespace NiceAttributes
{
    [AttributeUsage( AttributeTargets.Field, AllowMultiple = false, Inherited = true )]
    [Conditional("UNITY_EDITOR")]
    public class CurveRangeAttribute : DrawerAttribute
    {
        public Vector2 Min { get; private set; }
        public Vector2 Max { get; private set; }
        public NiceColor Color { get; private set; }

        public CurveRangeAttribute( Vector2 min, Vector2 max, NiceColor color = NiceColor.Clear, [CallerLineNumber] int lineNumber = 0 )
            : base( lineNumber )
        {
            Min = min;
            Max = max;
            Color = color;
        }

        public CurveRangeAttribute( NiceColor color, [CallerLineNumber] int lineNumber = 0 )
            : this( Vector2.zero, Vector2.one, color, lineNumber )
        {
        }

        public CurveRangeAttribute( float minX, float minY, float maxX, float maxY, NiceColor color = NiceColor.Clear, [CallerLineNumber] int lineNumber = 0 )
            : this( new Vector2( minX, minY ), new Vector2( maxX, maxY ), color, lineNumber )
        {
        }
    }
}
