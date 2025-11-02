using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using NiceAttributes.Model;

namespace NiceAttributes
{
    [AttributeUsage( AttributeTargets.Field, AllowMultiple = false, Inherited = true )]
    [Conditional("UNITY_EDITOR")]
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
