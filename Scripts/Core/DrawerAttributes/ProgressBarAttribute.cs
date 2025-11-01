using System;
using System.Runtime.CompilerServices;

namespace NiceAttributes
{
    [AttributeUsage( AttributeTargets.Field, AllowMultiple = false, Inherited = true )]
    public class ProgressBarAttribute : DrawerAttribute
    {
        public string Name { get; private set; }
        public float MaxValue { get; set; }
        public string MaxValueName { get; private set; }
        public NiceColor Color { get; private set; }

        public ProgressBarAttribute( string name, float maxValue, NiceColor color = NiceColor.Blue, [CallerLineNumber] int lineNumber = 0 )
            : base( lineNumber )
        {
            Name = name;
            MaxValue = maxValue;
            Color = color;
        }

        public ProgressBarAttribute( string name, string maxValueName, NiceColor color = NiceColor.Blue, [CallerLineNumber] int lineNumber = 0 )
            : base( lineNumber )
        {
            Name = name;
            MaxValueName = maxValueName;
            Color = color;
        }

        public ProgressBarAttribute( float maxValue, NiceColor color = NiceColor.Blue, [CallerLineNumber] int lineNumber = 0 )
            : this( "", maxValue, color, lineNumber )
        {
        }

        public ProgressBarAttribute( string maxValueName, NiceColor color = NiceColor.Blue, [CallerLineNumber] int lineNumber = 0 )
            : this( "", maxValueName, color, lineNumber )
        {
        }
    }
}
