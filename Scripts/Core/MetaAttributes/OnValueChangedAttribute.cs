using System;
using System.Runtime.CompilerServices;

namespace NiceAttributes
{
    [AttributeUsage( AttributeTargets.Field, AllowMultiple = true, Inherited = true )]
    public class OnValueChangedAttribute : MetaAttribute
    {
        public string CallbackName { get; private set; }

        public OnValueChangedAttribute( string callbackName, [CallerLineNumber] int lineNumber = 0 )
            : base( lineNumber )
        {
            CallbackName = callbackName;
        }
    }
}
