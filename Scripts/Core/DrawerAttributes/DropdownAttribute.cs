using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using NiceAttributes.Model;

namespace NiceAttributes
{
    [AttributeUsage( AttributeTargets.Field, AllowMultiple = false, Inherited = true )]
    [Conditional("UNITY_EDITOR")]
    public class DropdownAttribute : DrawerAttribute
    {
        public string ValuesName { get; private set; }

        public DropdownAttribute( string valuesName, [CallerLineNumber] int lineNumber = 0 )
            : base( lineNumber )
        {
            ValuesName = valuesName;
        }
    }
}
