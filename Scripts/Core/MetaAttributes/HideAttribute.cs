using System;
using System.Runtime.CompilerServices;

namespace NiceAttributes
{
    [AttributeUsage( AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Class | AttributeTargets.Struct )]
    public class HideAttribute : MetaAttribute
    {
        public HideAttribute( [CallerLineNumber] int lineNumber = 0 ) : base( lineNumber ) { }
    }
}