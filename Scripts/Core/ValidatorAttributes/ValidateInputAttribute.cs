using System;
using System.Runtime.CompilerServices;

namespace NiceAttributes
{
    [AttributeUsage( AttributeTargets.Field, AllowMultiple = false, Inherited = true )]
    public class ValidateInputAttribute : ValidatorAttribute
    {
        public string CallbackName { get; private set; }
        public string Message { get; private set; }

        public ValidateInputAttribute( string callbackName, string message = null, [CallerLineNumber] int lineNumber = 0 )
            : base( lineNumber )
        {
            CallbackName = callbackName;
            Message = message;
        }
    }
}
