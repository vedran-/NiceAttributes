namespace NiceAttributes.Interfaces
{
    public interface INiceAttribute
    {
        /// <summary>
        /// Line number in source code, of the field which has this Attribute.
        /// 
        /// We can use nice trick with [CallerLineNumber] to get line of the code, e.g.
        ///     public MetaAttribute( [CallerLineNumber] int lineNumber = 0 )
        ///
        /// And compiler will set lineNumber to be line of the code which used the attribute.
        /// https://stackoverflow.com/a/17998371/1111634
        /// </summary>
        public int LineNumber { get; }
    }
}
