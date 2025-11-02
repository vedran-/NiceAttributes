using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using NiceAttributes.Model;

namespace NiceAttributes
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    [Conditional("UNITY_EDITOR")]
    public class OnGUIAttribute : DrawerAttribute
    {
        public string PreDrawMethodName { get; set; }
        public string PostDrawMethodName { get; set; }

        public OnGUIAttribute( string postDrawMethodName = null, [CallerLineNumber] int lineNumber = 0 )
            : base( lineNumber )
        {
            PostDrawMethodName = postDrawMethodName;
        }
    }
}
