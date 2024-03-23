using System;
using System.Runtime.CompilerServices;

namespace NiceAttributes
{
    [AttributeUsage( AttributeTargets.Field, AllowMultiple = false, Inherited = true )]
    public class LayerAttribute : DrawerAttribute
    {
        public LayerAttribute( [CallerLineNumber] int lineNumber = 0 ) : base( lineNumber ) { }
    }
}