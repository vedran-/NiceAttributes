using System;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace NiceAttributes
{
    [AttributeUsage( AttributeTargets.Field, AllowMultiple = false, Inherited = true )]
    public class AnimatorParamAttribute : DrawerAttribute
    {
        public string AnimatorName { get; private set; }
        public AnimatorControllerParameterType? AnimatorParamType { get; private set; }

        public AnimatorParamAttribute( string animatorName, [CallerLineNumber] int lineNumber = 0 )
            : base( lineNumber )
        {
            AnimatorName = animatorName;
            AnimatorParamType = null;
        }

        public AnimatorParamAttribute( string animatorName, AnimatorControllerParameterType animatorParamType, [CallerLineNumber] int lineNumber = 0 )
            : base( lineNumber )
        {
            AnimatorName = animatorName;
            AnimatorParamType = animatorParamType;
        }
    }
}
