using System;
using System.Runtime.CompilerServices;

namespace NiceAttributes
{
    [AttributeUsage( AttributeTargets.Field, AllowMultiple = true, Inherited = true )]
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
