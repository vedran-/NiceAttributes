using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using NiceAttributes.Model;

namespace NiceAttributes
{
    [AttributeUsage( AttributeTargets.Field, AllowMultiple = false, Inherited = true )]
    [Conditional("UNITY_EDITOR")]
    public class RequiredAttribute : ValidatorAttribute
    {
        public string Message { get; private set; }

        public RequiredAttribute( string message = null, [CallerLineNumber] int lineNumber = 0 )
            : base( lineNumber )
        {
            Message = message;
        }
    }
}
