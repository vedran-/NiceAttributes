using System.Runtime.CompilerServices;

namespace NiceAttributes
{
    /// <summary>
    /// Special type of group attribute - if you have multiple variables in the same group,
    /// then all of them except one can use [Group] as a generic placeholder for a group,
    /// and just one of them needs to use [HorizontalGroup], [VerticalGroup], [TabGroup],
    /// or any other grouping type, any to define the actual group type. 
    /// </summary>
    public class GroupAttribute : BaseGroupAttribute
    {
        public GroupAttribute( string groupName = "", [CallerLineNumber] int lineNumber = 0 ) 
            : base( groupName, lineNumber ) {}
    }
}