using System;
using System.Runtime.CompilerServices;

namespace NiceAttributes
{
    [AttributeUsage( AttributeTargets.Field | AttributeTargets.Method, AllowMultiple = false, Inherited = true )]
    public class EnableIfAttribute : EnableIfAttributeBase
    {
        public EnableIfAttribute( string condition, [CallerLineNumber] int lineNumber = 0 )
            : base( condition, lineNumber )
        {
            Inverted = false;
        }

        //public EnableIfAttribute(EConditionOperator conditionOperator, params string[] conditions)
        //    : base(conditionOperator, conditions)
        //{
        //    Inverted = false;
        //}

        public EnableIfAttribute( string enumName, object enumValue, [CallerLineNumber] int lineNumber = 0 )
            : base( enumName, enumValue as Enum, lineNumber )
        {
            Inverted = false;
        }
    }
}
