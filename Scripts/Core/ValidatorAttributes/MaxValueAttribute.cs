using System;
using System.Runtime.CompilerServices;

namespace NiceAttributes
{
    [AttributeUsage( AttributeTargets.Field, AllowMultiple = false, Inherited = true )]
    public class MaxValueAttribute : ValidatorAttribute
    {
        public float MaxValue { get; private set; }

        public MaxValueAttribute( float maxValue, [CallerLineNumber] int lineNumber = 0 )
            : base( lineNumber )
        {
            MaxValue = maxValue;
        }

        public MaxValueAttribute( int maxValue, [CallerLineNumber] int lineNumber = 0 )
            : base( lineNumber )
        {
            MaxValue = maxValue;
        }
    }
}
