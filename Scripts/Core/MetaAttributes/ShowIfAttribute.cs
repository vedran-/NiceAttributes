using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace NiceAttributes
{
    [AttributeUsage( AttributeTargets.Field | AttributeTargets.Method, AllowMultiple = false, Inherited = true )]
    [Conditional("UNITY_EDITOR")]
    public class ShowIfAttribute : ShowIfAttributeBase
    {
        public ShowIfAttribute( string condition, [CallerLineNumber] int lineNumber = 0 )
            : base( condition, lineNumber )
        {
            Inverted = false;
        }

#if false
        public ShowIfAttribute(EConditionOperator conditionOperator, params string[] conditions)
            : base(conditionOperator, conditions)
        {
            Inverted = false;
        }
#endif

        public ShowIfAttribute( string enumName, object enumValue, [CallerLineNumber] int lineNumber = 0 )
            : base( enumName, enumValue as Enum, lineNumber )
        {
            Inverted = false;
        }
    }
}
