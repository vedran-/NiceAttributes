using System.Collections.Generic;
using System.Reflection;
using NiceAttributes.Editor.PropertyDrawers;
using NiceAttributes.Editor.Utility;
using UnityEditor;
using UnityEngine;

namespace NiceAttributes.Editor.Rendering
{
    public class ClassRenderer
    {
        private readonly List<ClassContext.ClassItem> _displayedMembers;
        private readonly object _targetObject;
        private readonly int _indentLevel;
        private static readonly Color BgNonSerialized = Color.Lerp(GUIUtil.GetDefaultBackgroundColor(), new Color32(127, 0, 0, 255), 0.15f);

        public ClassRenderer(List<ClassContext.ClassItem> displayedMembers, object targetObject, int indentLevel)
        {
            _displayedMembers = displayedMembers;
            _targetObject = targetObject;
            _indentLevel = indentLevel;
        }

        public void Render()
        {
            var oldIndent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = _indentLevel;

            ClassContext.GroupInfo[] lastOpenGroups = null;
            var hiddenGroups = new List<ClassContext.GroupInfo>();

            bool SetActiveGroups(ClassContext.GroupInfo[] newOpenedGroups)
            {
                if (newOpenedGroups != null) foreach (var g in newOpenedGroups)
                {
                    if (hiddenGroups.Contains(g))
                    {
                        return false;
                    }
                }

                if (lastOpenGroups != null)
                {
                    for (int i = lastOpenGroups.Length - 1; i >= 0; --i)
                    {
                        var shouldClose = newOpenedGroups == null
                            || i >= newOpenedGroups.Length
                            || lastOpenGroups[i] != newOpenedGroups[i];

                        if (shouldClose)
                        {
                            var isHidden = false;
                            for (int j = 0; j < i; j++)
                            {
                                if (hiddenGroups.Contains(lastOpenGroups[j]))
                                {
                                    isHidden = true;
                                    break;
                                }
                            }
                            if (isHidden)
                            {
                                hiddenGroups.Remove(lastOpenGroups[i]);
                            }
                            else
                            {
                                lastOpenGroups[i].groupAttribute?.FinishDrawingGroup();
                            }
                        }
                    }
                }

                if (newOpenedGroups != null)
                {
                    for (int i = 0; i < newOpenedGroups.Length; i++)
                    {
                        var shouldOpen = lastOpenGroups == null
                            || i >= lastOpenGroups.Length
                            || newOpenedGroups[i] != lastOpenGroups[i];

                        if (shouldOpen && newOpenedGroups[i].groupAttribute != null)
                        {
                            var shouldDraw = newOpenedGroups[i].groupAttribute.StartDrawingGroup();
                            if (!shouldDraw)
                            {
                                hiddenGroups.Add(newOpenedGroups[i]);
                                lastOpenGroups = newOpenedGroups;
                                return false;
                            }
                        }
                    }
                }

                lastOpenGroups = newOpenedGroups;
                return true;
            }

            foreach (var item in _displayedMembers)
            {
                var shouldDraw = SetActiveGroups(item.group?.groups);
                if (!shouldDraw) continue;
                DrawItem(item);
            }

            SetActiveGroups(null);
            EditorGUI.indentLevel = oldIndent;
        }

        private void DrawItem(ClassContext.ClassItem item)
        {
            var itemRect = EditorGUILayout.BeginVertical();

            if (item.serializedProperty == null && !(item.memberInfo is MethodInfo))
            {
                GUIUtil.FillRect(itemRect, BgNonSerialized);
            }

            if (item.errorMessage != null)
            {
                NiceEditorGUI.HelpBox_Layout(item.errorMessage, MessageType.Error);
            }

            List<string> preDrawList = null, postDrawList = null;
            if (item.serializedProperty == null && item.niceAttributes.Length > 0)
            {
                foreach (var niceAttribute in item.niceAttributes)
                {
                    if (niceAttribute is OnGUIAttribute onGuiAttribute)
                    {
                        if (!string.IsNullOrWhiteSpace(onGuiAttribute.PreDrawMethodName))
                        {
                            preDrawList ??= new List<string>();
                            preDrawList.Add(onGuiAttribute.PreDrawMethodName);
                        }
                        if (!string.IsNullOrWhiteSpace(onGuiAttribute.PostDrawMethodName))
                        {
                            postDrawList ??= new List<string>();
                            postDrawList.Add(onGuiAttribute.PostDrawMethodName);
                        }
                    }
                }
            }

            if (preDrawList != null)
            {
                foreach (var methodName in preDrawList)
                {
                    OnGUIPropertyDrawer.RunGUIMethod(_targetObject, methodName);
                }
            }

            if (item.classContext != null)
            {
                if (item.serializedProperty != null)
                {
                    EditorGUILayout.PropertyField(item.serializedProperty, false);
                    item.foldedOut = item.serializedProperty.isExpanded;
                }
                else
                {
                    item.foldedOut = EditorGUILayout.Foldout(item.foldedOut, ObjectNames.NicifyVariableName(item.memberInfo.Name), true);
                }

                if (item.foldedOut)
                {
                    item.classContext.Draw();
                }
            }
            else if (item.serializedProperty != null)
            {
                if (item.serializedProperty.name == "m_Script")
                {
                    using (new EditorGUI.DisabledScope(disabled: true))
                    {
                        EditorGUILayout.PropertyField(item.serializedProperty);
                    }
                }
                else
                {
                    NiceEditorGUI.PropertyField_Layout(item.serializedProperty, includeChildren: true);
                }
            }
            else
            {
                if (item.memberInfo is FieldInfo field)
                {
                    NiceEditorGUI.NonSerializedField_Layout(_targetObject, field);
                }
                else if (item.memberInfo is PropertyInfo property)
                {
                    NiceEditorGUI.NativeProperty_Layout(_targetObject, property);
                }
                else if (item.memberInfo is MethodInfo method)
                {
                    NiceEditorGUI.Button(_targetObject, method);
                }
            }

            if (postDrawList != null)
            {
                foreach (var methodName in postDrawList)
                {
                    OnGUIPropertyDrawer.RunGUIMethod(_targetObject, methodName);
                }
            }

            EditorGUILayout.EndVertical();
        }
    }
}
