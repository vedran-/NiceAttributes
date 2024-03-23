using UnityEngine;
using System.Reflection;
using System.Collections.Generic;

namespace NiceAttributes.Editor
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

            List<bool> conditionValues = PropertyUtility.GetConditionValues(target, enableIfAttribute.Conditions);
            if( conditionValues.Count > 0 )
            {
                bool enabled = PropertyUtility.GetConditionsFlag(conditionValues, enableIfAttribute.ConditionOperator, enableIfAttribute.Inverted);
                return enabled;
            } else
            {
                string message = enableIfAttribute.GetType().Name + $" needs a valid boolean condition field, property or method name to work. [{string.Join(", ", enableIfAttribute.Conditions)}]\n";
                Debug.LogWarning( message, target as UnityEngine.Object );

                return false;
            }
        }

        public static bool IsVisible( object target, MethodInfo method )
        {
            ShowIfAttributeBase showIfAttribute = method.GetCustomAttribute<ShowIfAttributeBase>();
            if (showIfAttribute == null)
            {
                return true;
            }

            List<bool> conditionValues = PropertyUtility.GetConditionValues(target, showIfAttribute.Conditions);
            if (conditionValues.Count > 0)
            {
                bool enabled = PropertyUtility.GetConditionsFlag(conditionValues, showIfAttribute.ConditionOperator, showIfAttribute.Inverted);
                return enabled;
            }
            else
            {
                string message = showIfAttribute.GetType().Name + " needs a valid boolean condition field, property or method name to work";
                Debug.LogWarning( message, target as UnityEngine.Object );

                return false;
            }
        }
    }
}
