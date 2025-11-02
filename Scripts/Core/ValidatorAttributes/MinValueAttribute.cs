using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using NiceAttributes.Model;

namespace NiceAttributes
{
    [AttributeUsage( AttributeTargets.Field, AllowMultiple = false, Inherited = true )]
    [Conditional("UNITY_EDITOR")]
    public class MinValueAttribute : ValidatorAttribute
    {
        public float MinValue { get; private set; }

        public MinValueAttribute( float minValue, [CallerLineNumber] int lineNumber = 0 )
            : base( lineNumber )
        {
            MinValue = minValue;
        }

        public MinValueAttribute( int minValue, [CallerLineNumber] int lineNumber = 0 )
            : base( lineNumber )
        {
            MinValue = minValue;
        }
    }
}
