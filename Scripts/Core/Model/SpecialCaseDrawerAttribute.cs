using System;
using NiceAttributes.Interfaces;

namespace NiceAttributes.Model
{
    public class SpecialCaseDrawerAttribute : Attribute, INiceAttribute
    {
        public int LineNumber { get; set; }

        /// <summary>
        /// In your inherited class, please use [CallerLineNumber] to get line number and forward it to SpecialCaseDrawerAttribute,
        /// e.g.
        ///     public VerticalGroup( [CallerLineNumber] int lineNumber = 0 ) : base( lineNumber )
        /// That way all the fields, properties and methods (buttons) will be ordered exactly as they are in the source file.
        /// </summary>
        /// <param name="lineNumber">Line number in source code file, where current Attribute was used</param>
        public SpecialCaseDrawerAttribute( int lineNumber )
        {
            LineNumber = lineNumber;
        }
    }
}
