using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using NiceAttributes.Model;

namespace NiceAttributes
{
    [AttributeUsage( AttributeTargets.Field, AllowMultiple = false, Inherited = true )]
    [Conditional("UNITY_EDITOR")]
    public class MinMaxSliderAttribute : DrawerAttribute
    {
        public float MinValue { get; private set; }
        public float MaxValue { get; private set; }

        public MinMaxSliderAttribute( float minValue, float maxValue, [CallerLineNumber] int lineNumber = 0 )
            : base( lineNumber )
        {
            MinValue = minValue;
            MaxValue = maxValue;
        }
    }
}
