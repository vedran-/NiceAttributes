using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace NiceAttributes
{
    [AttributeUsage( AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Method, AllowMultiple = true )]
    [Conditional("UNITY_EDITOR")]
    public class HorizontalGroupAttribute : BaseGroupAttribute
    {
        public HorizontalGroupAttribute( string groupName = "", [CallerLineNumber] int lineNumber = 0 ) 
            : base( groupName, lineNumber ) {}
    }
}