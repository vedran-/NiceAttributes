using System;
using System.Runtime.CompilerServices;

namespace NiceAttributes
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public class SceneAttribute : DrawerAttribute
    {
        public SceneAttribute( [CallerLineNumber] int lineNumber = 0 ) : base( lineNumber ) {}
    }
}