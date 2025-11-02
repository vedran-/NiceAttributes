using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace NiceAttributes
{
    [AttributeUsage( AttributeTargets.Field | AttributeTargets.Method, AllowMultiple = false, Inherited = true )]
    [Conditional("UNITY_EDITOR")]
    public class DisableIfAttribute : EnableIfAttributeBase
    {
        public DisableIfAttribute( string condition, [CallerLineNumber] int lineNumber = 0 )
            : base( condition, lineNumber )
        {
            Inverted = true;
        }

#if false
        public DisableIfAttribute(EConditionOperator conditionOperator, params string[] conditions, [CallerLineNumber] int lineNumber = 0 )
            : base(conditionOperator, conditions, lineNumber)
        {
            Inverted = true;
        }
#endif

        public DisableIfAttribute( string enumName, object enumValue, [CallerLineNumber] int lineNumber = 0 )
            : base( enumName, enumValue as Enum, lineNumber )
        {
            Inverted = true;
        }
    }
}
