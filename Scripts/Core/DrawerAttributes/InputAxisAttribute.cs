using System;
using System.Runtime.CompilerServices;

namespace NiceAttributes
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public class InputAxisAttribute : DrawerAttribute
    {
        public InputAxisAttribute( [CallerLineNumber] int lineNumber = 0 ) : base( lineNumber ) { }
    }
}
