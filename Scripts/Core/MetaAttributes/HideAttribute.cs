using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using NiceAttributes.Model;

namespace NiceAttributes
{
    [AttributeUsage( AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Class | AttributeTargets.Struct )]
    [Conditional("UNITY_EDITOR")]
    public class HideAttribute : MetaAttribute
    {
        public HideAttribute( [CallerLineNumber] int lineNumber = 0 ) : base( lineNumber ) { }
    }
}