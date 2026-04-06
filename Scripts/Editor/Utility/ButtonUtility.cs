using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace NiceAttributes.Editor.Utility
{
    public static class ButtonUtility
    {
        public static bool IsEnabled( object target, MethodInfo method )
        {
            EnableIfAttributeBase enableIfAttribute = method.GetCustomAttribute<EnableIfAttributeBase>();
            if( enableIfAttribute == null )
            {
                return true;
            }

            return ConditionalEvaluator.Evaluate(enableIfAttribute, target);
        }

        public static bool IsVisible( object target, MethodInfo method )
        {
            ShowIfAttributeBase showIfAttribute = method.GetCustomAttribute<ShowIfAttributeBase>();
            if (showIfAttribute == null)
            {
                return true;
            }

            return ConditionalEvaluator.Evaluate(showIfAttribute, target);
        }
    }
}
