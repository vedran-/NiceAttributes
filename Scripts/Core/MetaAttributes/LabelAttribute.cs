using System;
using System.Runtime.CompilerServices;

namespace NiceAttributes
{
    [AttributeUsage( AttributeTargets.Field, AllowMultiple = false, Inherited = true )]
    public class LabelAttribute : MetaAttribute
    {
        public string Label { get; private set; }

        public LabelAttribute( string label, [CallerLineNumber] int lineNumber = 0 )
            : base( lineNumber )
        {
            Label = label;
        }
    }
}
