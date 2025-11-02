using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using NiceAttributes.Model;

namespace NiceAttributes
{
    [AttributeUsage( AttributeTargets.Field, AllowMultiple = true, Inherited = true )]
    [Conditional("UNITY_EDITOR")]
    public class InfoBoxAttribute : DrawerAttribute
    {
        public string       Text { get; private set; }
        public InfoBoxType  Type { get; private set; }

        public InfoBoxAttribute( string text, InfoBoxType type = InfoBoxType.Info, [CallerLineNumber] int lineNumber = 0 )
            : base( lineNumber )
        {
            Text = text;
            Type = type;
        }
    }
}
