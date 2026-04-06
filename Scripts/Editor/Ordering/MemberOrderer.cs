using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NiceAttributes.Editor;

namespace NiceAttributes.Editor.Ordering
{
    public static class MemberOrderer
    {
        /// <summary>
        /// Orders members by line number, interpolating line numbers for members that don't have them.
        /// For members which don't have line numbers, select a number between their neighbours which do have line numbers.
        /// But make sure that only use neighbours of the same type!
        /// </summary>
        public static IEnumerable<ClassContext.ClassItem> Order(List<ClassContext.ClassItem> members)
        {
            MemberTypes lastType = 0;
            float minValue = -1;
            for( int idx = 0; idx < members.Count; idx++ )
            {
                var m = members[idx];
                if( m.lineNumber < 0 )
                {
                    // If member type changed from last member, then reset last min value
                    if( m.memberInfo.MemberType != lastType ) minValue = -1;

                    // Find next member of the same type, which has line number set.
                    var maxValue = -1f;
                    for( int i = idx + 1; i < members.Count; i++ ) {
                        if( members[i].memberInfo.MemberType != m.memberInfo.MemberType ) break;   // Not in our group any more, stop the search
                        if( members[i].lineNumber < 0 ) continue;
                        maxValue = members[i].lineNumber;
                        break;
                    }

                    float GetNewLineNumber()
                    {
                        // If no member of this type has line number set, return number
                        if( minValue < 0 && maxValue < 0 ) return (int)m.memberInfo.MemberType * 10f;
                        if( minValue < 0 && maxValue >= 0 ) return maxValue - 1f;
                        if( maxValue < 0 && minValue >= 0 ) return minValue + 1f;
                        return (minValue + maxValue) / 2f;
                    }

                    // Set new calculated line number - this will be used for sorting
                    m.lineNumber = GetNewLineNumber();
                }

                lastType = m.memberInfo.MemberType;
                minValue = m.lineNumber;
            }

            // Finally, order all members by line numbers
            return members.OrderBy( m => m.lineNumber );
        }
    }
}
