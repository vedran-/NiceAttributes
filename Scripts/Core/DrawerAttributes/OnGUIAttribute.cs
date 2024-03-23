using System;
using System.Runtime.CompilerServices;

namespace NiceAttributes
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
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
