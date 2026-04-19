using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace NiceAttributes
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    [Conditional("UNITY_EDITOR")]
    public class FoldoutAttribute : BaseGroupAttribute
    {
        string id => GroupName;

        public FoldoutAttribute(string groupName = "", [CallerLineNumber] int lineNumber = 0)
            : base(groupName, lineNumber)
        {
        }
    }
}