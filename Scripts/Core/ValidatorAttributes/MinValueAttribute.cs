using System;
using System.Runtime.CompilerServices;

namespace NiceAttributes
{
    [AttributeUsage( AttributeTargets.Field, AllowMultiple = false, Inherited = true )]
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
