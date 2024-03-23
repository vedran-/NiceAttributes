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
        public EColor Color { get; private set; }

        public ProgressBarAttribute( string name, float maxValue, EColor color = EColor.Blue, [CallerLineNumber] int lineNumber = 0 )
            : base( lineNumber )
        {
            Name = name;
            MaxValue = maxValue;
            Color = color;
        }

        public ProgressBarAttribute( string name, string maxValueName, EColor color = EColor.Blue, [CallerLineNumber] int lineNumber = 0 )
            : base( lineNumber )
        {
            Name = name;
            MaxValueName = maxValueName;
            Color = color;
        }

        public ProgressBarAttribute( float maxValue, EColor color = EColor.Blue, [CallerLineNumber] int lineNumber = 0 )
            : this( "", maxValue, color, lineNumber )
        {
        }

        public ProgressBarAttribute( string maxValueName, EColor color = EColor.Blue, [CallerLineNumber] int lineNumber = 0 )
            : this( "", maxValueName, color, lineNumber )
        {
        }
    }
}
