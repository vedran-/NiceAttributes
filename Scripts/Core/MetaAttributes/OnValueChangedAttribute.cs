using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using NiceAttributes.Model;

namespace NiceAttributes
{
    [AttributeUsage( AttributeTargets.Field, AllowMultiple = true, Inherited = true )]
    [Conditional("UNITY_EDITOR")]
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
