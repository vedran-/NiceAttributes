using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace NiceAttributes
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    [Conditional("UNITY_EDITOR")]
    public class BoxGroupAttribute : BaseGroupAttribute
    {
        public BoxGroupAttribute( string groupName = "", bool showTitle = true, [CallerLineNumber] int lineNumber = 0 ) 
            : base( groupName, lineNumber )
        {
            ShowTitle = showTitle;
        }
    }
}