using System.Reflection;
using UnityEditor;

namespace NiceAttributes.Editor
{
    public partial class ClassContext
    {
        private class ClassItem
        {
            public MemberInfo           memberInfo;
            public INiceAttribute[]     niceAttributes;
            public float                lineNumber;             // Line number in source code

            public GroupInfo            group;                  // To which group does item belong
            public SerializedProperty   serializedProperty = null;  // If item is serialized, this will be set
            public string               errorMessage = null;    // Used to display warnings/errors to user
            public bool                 foldedOut = true;       // For non-serialized members, we have to track if they're folded

            /// <summary>
            /// If member is a class or struct, it can have its own context for displaying its children.
            /// Othewise it will be null.
            /// </summary>
            public ClassContext         classContext = null;
        }
    }
}