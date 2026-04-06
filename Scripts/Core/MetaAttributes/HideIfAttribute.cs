using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace NiceAttributes
{
    [AttributeUsage( AttributeTargets.Field | AttributeTargets.Method, AllowMultiple = false, Inherited = true )]
    [Conditional("UNITY_EDITOR")]
    public class HideIfAttribute : ShowIfAttributeBase
    {
        public HideIfAttribute( string condition, [CallerLineNumber] int lineNumber = 0 )
            : base( condition, lineNumber )
        {
            Inverted = true;
        }

        public HideIfAttribute( string enumName, object enumValue, [CallerLineNumber] int lineNumber = 0 )
            : base( enumName, enumValue as Enum, lineNumber )
        {
            Inverted = true;
        }
    }
}
