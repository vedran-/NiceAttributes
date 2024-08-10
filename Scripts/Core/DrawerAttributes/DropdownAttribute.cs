using System;
using System.Runtime.CompilerServices;

namespace NiceAttributes
{
    [AttributeUsage( AttributeTargets.Field, AllowMultiple = false, Inherited = true )]
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
