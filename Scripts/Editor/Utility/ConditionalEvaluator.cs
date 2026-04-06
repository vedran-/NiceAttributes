using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace NiceAttributes.Editor.Utility
{
    public static class ConditionalEvaluator
    {
        public static bool Evaluate(IConditionalAttribute attr, object target)
        {
            if (attr == null)
            {
                return true;
            }

            // deal with enum conditions
            if (attr.EnumValue != null)
            {
                Enum value = PropertyUtility.GetEnumValue(target, attr.Conditions[0]);
                if (value != null)
                {
                    bool matched = value.GetType().GetCustomAttribute<FlagsAttribute>() == null
                        ? attr.EnumValue.Equals(value)
                        : value.HasFlag(attr.EnumValue);

                    return matched != attr.Inverted;
                }

                string message = attr.GetType().Name + " needs a valid enum field, property or method name to work";
                Debug.LogWarning(message, target as UnityEngine.Object);

                return false;
            }

            // deal with normal conditions
            List<bool> conditionValues = PropertyUtility.GetConditionValues(target, attr.Conditions);
            if (conditionValues.Count > 0)
            {
                bool enabled = PropertyUtility.GetConditionsFlag(conditionValues, attr.ConditionOperator, attr.Inverted);
                return enabled;
            }
            else
            {
                string message = attr.GetType().Name + " needs a valid boolean condition field, property or method name to work";
                Debug.LogWarning(message, target as UnityEngine.Object);

                return false;
            }
        }
    }
}
