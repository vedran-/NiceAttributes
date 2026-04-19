using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using NiceAttributes.Editor;
using NiceAttributes.Interfaces;
using NiceAttributes.Editor.Utility;
using UnityEditor;
using UnityEngine;

namespace NiceAttributes.Editor.Discovery
{
    public class MemberDiscoverer
    {
        private const BindingFlags AllBindingFields = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.DeclaredOnly;
        private static readonly Type[] SkipTypes = new Type[] { typeof(UnityEngine.Object), typeof(ScriptableObject), typeof(MonoBehaviour) };

        public bool HasNiceAttributes { get; private set; } = false;

        private readonly object _targetObject;
        private readonly int _indentLevel;
        private readonly Action<Type, object, int> _createChildContext;

        public MemberDiscoverer(object targetObject, int indentLevel, Action<Type, object, int> createChildContext)
        {
            _targetObject = targetObject;
            _indentLevel = indentLevel;
            _createChildContext = createChildContext;
        }

        public List<ClassContext.ClassItem> GetAllMembers(Type classType, Type[] additionalSkipTypes)
        {
            var members = new List<ClassContext.ClassItem>();

            var types = new List<Type>() { classType };
            while (types.Last().BaseType != null)
            {
                var bt = types.Last().BaseType;
                if (SkipTypes.Contains(bt)) break;
                if (additionalSkipTypes != null && additionalSkipTypes.Contains(bt)) break;
                types.Add(bt);
            }

            void AddMember(MemberInfo member, ClassContext memberClassContext)
            {
                var n = new ClassContext.ClassItem()
                {
                    memberInfo = member,
                    niceAttributes = member.GetInterfaceAttributes<INiceAttribute>().ToArray(),
                    classContext = memberClassContext
                };
                if (n.niceAttributes.Length > 0)
                {
                    HasNiceAttributes = true;
                }

                n.lineNumber = n.niceAttributes.Length > 0 ? n.niceAttributes[0].LineNumber : -1f;

                members.Add(n);
            }

            for (int i = types.Count - 1; i >= 0; i--)
            {
                var typeMembers = types[i]
                    .GetMembers(AllBindingFields)
                    .Where(m => IsVisible(m));

                foreach (var m in typeMembers)
                {
                    var fieldInfo = m as FieldInfo;
                    var propInfo = m as PropertyInfo;
                    var methodInfo = m as MethodInfo;
                    var memberType = fieldInfo != null ? fieldInfo.FieldType
                        : propInfo != null ? propInfo.PropertyType
                        : methodInfo != null ? methodInfo.ReturnType
                        : null;

                    if (propInfo != null && !propInfo.CanRead) continue;

                    ClassContext memberClassContext = null;
                    if (memberType != null && !memberType.IsArray && !memberType.IsGenericList())
                    {
                        var treatAsClassOrStruct = memberType.IsClassOrStruct()
                                                   && memberType != typeof(string);

                        if (treatAsClassOrStruct)
                        {
                            if (Attribute.IsDefined(memberType, typeof(HideAttribute)))
                            {
                                HasNiceAttributes = true;
                                continue;
                            }

                            var useExpandedView =
                                (Attribute.IsDefined(memberType, typeof(SerializableAttribute)) || Attribute.IsDefined(memberType, typeof(ShowAttribute)))
                                && !memberType.IsSubclassOfRawGeneric(typeof(UnityEngine.Object));

                            if (useExpandedView)
                            {
                                var obj = propInfo != null ? propInfo.GetValue(_targetObject, null)
                                    : fieldInfo != null ? fieldInfo.GetValue(_targetObject)
                                    : null;
                                if (obj == null)
                                {
                                    Debug.LogWarning($"Object for member {m.Name} is null. Parent object was {_targetObject}");
                                }
                                else
                                {
                                    _createChildContext(memberType, obj, _indentLevel + 1);
                                    if (HasNiceAttributes) HasNiceAttributes = true;
                                }
                            }
                        }
                    }

                    AddMember(m, memberClassContext);
                }
            }

            return members;
        }

        private bool IsVisible(MemberInfo mt)
        {
            if (Attribute.IsDefined(mt, typeof(HideAttribute)))
            {
                HasNiceAttributes = true;
                return false;
            }

            if (mt.MemberType == MemberTypes.Property)
            {
                return Attribute.IsDefined(mt, typeof(ShowAttribute))
                       || ReflectionUtility.IsPropertySerialized(mt as PropertyInfo);
            }
            if (mt.MemberType == MemberTypes.Method) return Attribute.IsDefined(mt, typeof(ButtonAttribute));
            if (mt is not FieldInfo fi) return false;

            if (Attribute.IsDefined(fi, typeof(ShowAttribute))) return true;
            if (fi.IsStatic) return false;
            if (fi.GetCustomAttribute<CompilerGeneratedAttribute>() != null) return false;

            var isVisible = (fi.IsPublic && !Attribute.IsDefined(fi, typeof(NonSerializedAttribute)))
                            || Attribute.IsDefined(fi, typeof(SerializeField));

            var fieldType = fi.FieldType;
            if (isVisible)
            {
                if (fieldType.IsArray) fieldType = fieldType.GetElementType();
                if (fieldType.IsGenericList()) fieldType = fieldType.GetGenericArguments()[0];

                if (fieldType.IsClassOrStruct())
                {
                    isVisible = Attribute.IsDefined(fieldType, typeof(SerializableAttribute))
                                || Attribute.IsDefined(fieldType, typeof(ShowAttribute))
                                || fieldType.IsSubclassOfRawGeneric(typeof(UnityEngine.Object))
                                || ReflectionUtility.UnitySpecialTypes.Any(t => t == fieldType);
                }
            }

            return isVisible;
        }
    }
}
