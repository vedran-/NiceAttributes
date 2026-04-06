using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace NiceAttributes.Editor.Utility
{
    public static class NonSerializedFieldRenderer
    {
        private static Dictionary<string, bool> _collectionFoldouts = new Dictionary<string, bool>();

        internal static void NativeProperty_Layout(object target, PropertyInfo property)
        {
            var value = property.GetValue(target, null);
            var label = new GUIContent(ObjectNames.NicifyVariableName(property.Name));
            if (!Field_Layout(value, property.PropertyType, label, !property.CanWrite, out var outValue))
            {
                var warning = $"{nameof(NiceAttributes.Model.ShowAttribute)} doesn't support {property.PropertyType.Name} types";
                HelpBoxRenderer.HelpBox_Layout(warning, MessageType.Warning, context: target as UnityEngine.Object);
                return;
            }

            if (property.CanWrite)
            {
                property.SetValue(target, outValue, null);
            }
        }

        internal static void NonSerializedField_Layout(object target, FieldInfo field)
        {
            var value = field.GetValue(target);
            var label = new GUIContent(ObjectNames.NicifyVariableName(field.Name));
            if (!Field_Layout(value, field.FieldType, label, false, out var outValue))
            {
                var warning = $"{nameof(NiceAttributes.Model.ShowAttribute)} doesn't support {field.FieldType.Name} types";
                HelpBoxRenderer.HelpBox_Layout(warning, MessageType.Warning, context: target as UnityEngine.Object);
                return;
            }

            if (outValue == null ? value != null : !outValue.Equals(value))
            {
                field.SetValue(target, outValue);
            }
        }

        private static bool Field_Layout(object value, Type valueType, GUIContent label, bool readOnly, out object outValue)
        {
            using (new EditorGUI.DisabledScope(disabled: readOnly))
            {
                var isDrawn = true;

                if (valueType == null)
                {
                    outValue = null;
                    return false;
                }

                if (typeof(UnityEngine.Object).IsAssignableFrom(valueType))
                {
                    outValue = EditorGUILayout.ObjectField(label, (UnityEngine.Object)value, valueType, true);
                }
                else if (valueType == typeof(int))
                {
                    outValue = (int)EditorGUILayout.IntField(label, (int)value);
                }
                else if (valueType == typeof(bool))
                {
                    outValue = (bool)EditorGUILayout.Toggle(label, (bool)value);
                }
                else if (valueType == typeof(string))
                {
                    outValue = (string)EditorGUILayout.TextField(label, (string)value);
                }
                else if (valueType == typeof(float))
                {
                    outValue = (float)EditorGUILayout.FloatField(label, (float)value);
                }
                else if (valueType == typeof(double))
                {
                    outValue = (double)EditorGUILayout.DoubleField(label, (double)value);
                }
                else if (valueType == typeof(short))
                {
                    outValue = (short)EditorGUILayout.IntField(label, (short)value);
                }
                else if (valueType == typeof(ushort))
                {
                    outValue = (ushort)EditorGUILayout.IntField(label, (ushort)value);
                }
                else if (valueType == typeof(uint))
                {
                    outValue = (uint)EditorGUILayout.LongField(label, (uint)value);
                }
                else if (valueType == typeof(long))
                {
                    outValue = (long)EditorGUILayout.LongField(label, (long)value);
                }
                else if (valueType == typeof(ulong))
                {
                    var val = EditorGUILayout.TextField(label, ((ulong)value).ToString());
                    if (ulong.TryParse(val, out var outVal))
                    {
                        outValue = outVal;
                    }
                    else
                    {
                        outValue = value;
                    }
                }
                else if (valueType == typeof(Vector2))
                {
                    outValue = EditorGUILayout.Vector2Field(label, (Vector2)value);
                }
                else if (valueType == typeof(Vector3))
                {
                    outValue = EditorGUILayout.Vector3Field(label, (Vector3)value);
                }
                else if (valueType == typeof(Vector4))
                {
                    outValue = EditorGUILayout.Vector4Field(label, (Vector4)value);
                }
                else if (valueType == typeof(Vector2Int))
                {
                    outValue = EditorGUILayout.Vector2IntField(label, (Vector2Int)value);
                }
                else if (valueType == typeof(Vector3Int))
                {
                    outValue = EditorGUILayout.Vector3IntField(label, (Vector3Int)value);
                }
                else if (valueType == typeof(Color))
                {
                    outValue = EditorGUILayout.ColorField(label, (Color)value);
                }
                else if (valueType == typeof(Bounds))
                {
                    outValue = EditorGUILayout.BoundsField(label, (Bounds)value);
                }
                else if (valueType == typeof(Rect))
                {
                    outValue = EditorGUILayout.RectField(label, (Rect)value);
                }
                else if (valueType == typeof(RectInt))
                {
                    outValue = EditorGUILayout.RectIntField(label, (RectInt)value);
                }
                else if (valueType.BaseType == typeof(Enum))
                {
                    outValue = EditorGUILayout.EnumPopup(label, (Enum)value);
                }
                else if (valueType.BaseType == typeof(System.Reflection.TypeInfo))
                {
                    outValue = EditorGUILayout.TextField(label, value.ToString());
                }
                else if (typeof(ICollection).IsAssignableFrom(valueType))
                {
                    (isDrawn, outValue) = HandleCollection(value, label);
                }
                else
                {
                    isDrawn = false;
                    outValue = null;
                }

                return isDrawn;
            }
        }

        private static (bool isDrawn, object outValue) HandleCollection(object value, GUIContent label)
        {
            var valueType = value.GetType();
            var array = valueType.IsArray ? (Array)value : null;
            var genericCollection = valueType.IsGenericType ? value as ICollection : null;

            var length = array != null ? array.Length
                : genericCollection != null ? genericCollection.Count : -1;

            var elementType = valueType.GetElementType();

            if (length < 0)
            {
                HelpBoxRenderer.HelpBox_Layout($"Invalid ICollection type '{valueType.Name}'", MessageType.Error);
                return (false, value);
            }

            var foldoutKey = valueType.FullName + "_" + label.text;
            if (!_collectionFoldouts.TryGetValue(foldoutKey, out var foldedOut))
            {
                foldedOut = true;
            }

            var folded = EditorGUILayout.Foldout(foldedOut, label, true);
            if (folded != foldedOut)
            {
                _collectionFoldouts[foldoutKey] = folded;
            }
            if (!folded) return (true, value);

            for (int i = 0; i < length; i++)
            {
                var item = array != null ? array.GetValue(i) : genericCollection.Cast<object>().ElementAt(i);

                if (Field_Layout(item, elementType, new GUIContent($"Element {i}"), false, out var outValue))
                {
                    if (outValue == value) continue;

                    if (array != null) array.SetValue(outValue, i);
                    else if (genericCollection != null)
                    {
                        if (genericCollection is IList list)
                        {
                            list[i] = outValue;
                        }
                        else
                        {
                            var tempCollection = Activator.CreateInstance(valueType, true);
                            var addMethod = valueType.GetMethod("Add");
                            var count = 0;
                            foreach (var element in genericCollection)
                            {
                                addMethod.Invoke(tempCollection, new[] { count == i ? outValue : element });
                                count++;
                            }
                            genericCollection = tempCollection as ICollection;
                        }
                    }
                }
            }

            return (true, value);
        }
    }
}
