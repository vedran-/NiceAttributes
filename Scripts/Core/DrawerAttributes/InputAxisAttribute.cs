using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using NiceAttributes.Model;

namespace NiceAttributes
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    [Conditional("UNITY_EDITOR")]
    public class InputAxisAttribute : DrawerAttribute
    {
        public InputAxisAttribute( [CallerLineNumber] int lineNumber = 0 ) : base( lineNumber ) { }
    }
}
