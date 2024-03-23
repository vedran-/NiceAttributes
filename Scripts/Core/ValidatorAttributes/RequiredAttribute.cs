using System;
using System.Runtime.CompilerServices;

namespace NiceAttributes
{
    [AttributeUsage( AttributeTargets.Field, AllowMultiple = false, Inherited = true )]
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
