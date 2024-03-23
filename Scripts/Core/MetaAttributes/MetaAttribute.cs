using System;

namespace NiceAttributes
{
    public abstract class MetaAttribute : Attribute, INiceAttribute
    {
        public int LineNumber { get; set; }

        /// <summary>
        /// In your class, please use [CallerLineNumber] to get line number and forward it to MetaAttribute,
        /// e.g.
        ///     public VerticalGroup( [CallerLineNumber] int lineNumber = 0 ) : base( lineNumber )
        /// That way all the fields, properties and methods (buttons) will be ordered exactly as they are in the source file.
        /// </summary>
        /// <param name="lineNumber">Line number in source code file, where current Attribute was used</param>
        public MetaAttribute( int lineNumber )
        {
            LineNumber = lineNumber;
        }
    }
}
