using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using NiceAttributes.Editor.Discovery;
using NiceAttributes.Editor.Grouping;
using NiceAttributes.Editor.Ordering;
using NiceAttributes.Editor.PropertyDrawers;
using NiceAttributes.Editor.Utility;
using NiceAttributes.Interfaces;
using UnityEditor;
using UnityEngine;

namespace NiceAttributes.Editor
{
    public partial class ClassContext
    {
        private const BindingFlags AllBindingFields = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.DeclaredOnly;
        private static readonly Type[] SkipTypes = new Type[] { typeof(UnityEngine.Object), typeof(ScriptableObject), typeof(MonoBehaviour) };
        private static readonly Color BgNonSerialized = Color.Lerp(GUIUtil.GetDefaultBackgroundColor(), new Color32(127, 0, 0, 255), 0.15f);
        
        public bool             HasNiceAttributes { get; private set; } = false;

        private List<ClassItem> _displayedMembers = null;
        private object          _targetObject;
        private int             _indentLevel;


        // Private constructor - call CreateContext() to create a new instance
        private ClassContext() {}


        /// <summary>
        /// Get all members of the class and all its base classes.
        /// It also recursively visits subclasses/substructs
        /// </summary>
        /// <param name="classType"></param>
        /// <param name="targetObject"></param>
        /// <param name="indentLevel"></param>
        /// <param name="additionalSkipTypes"></param>
        /// <param name="alwaysUseNiceInspector">If set, it will always use NiceAttribute's Inspector, even if there are not NiceAttributes used in the class</param>
        public static ClassContext CreateContext(Type classType, object targetObject, int indentLevel,
            Type[] additionalSkipTypes = null, bool alwaysUseNiceInspector = false)
        {
            var ctx = new ClassContext();
            if (alwaysUseNiceInspector) ctx.HasNiceAttributes = true;
            ctx._targetObject = targetObject;
            ctx._indentLevel = indentLevel;

            // Get all class members
            var discoverer = new MemberDiscoverer(targetObject, indentLevel, (memberType, obj, childIndent) =>
            {
                var childCtx = CreateContext(memberType, obj, childIndent, additionalSkipTypes);
                if (childCtx.HasNiceAttributes) ctx.HasNiceAttributes = true;
            });
            var members = discoverer.GetAllMembers(classType, additionalSkipTypes);
            if (discoverer.HasNiceAttributes) ctx.HasNiceAttributes = true;

            //Debug.Log( $"All members {ctx.hasNiceAttributes} for {classType.Name}:\n - " + string.Join( "\n - ", ctx.members.Select( m => $"{m.memberInfo} - {m.memberInfo.GetInterfaceAttributes<INiceAttribute>().FirstOrDefault()?.LineNumber} >> [{ string.Join( ", ", m.niceAttributes.Select( a => a.GetType() ) )}]" ) ) );

            // If class (or its subclasses) don't have any INiceAttribute, stop further processing
            // - we'll then use default inspector instead of our anyhow
            if (!ctx.HasNiceAttributes) return ctx;

            // Order class members, as they appear in the source code file
            var orderedMembers = MemberOrderer.Order(members);

            var displayTree = GroupResolver.BuildDisplayTree(orderedMembers);
            ctx._displayedMembers = displayTree.members;

            TabGroupInitializer.Initialize(displayTree.groups);

            return ctx;
        }

        
        internal static void ConnectWithSerializedProperties( ClassContext ctx, SerializedProperty property )
        {
            var parentPath = property.propertyPath;
            if( !property.NextVisible( true ) ) return;

            do
            {
                if( !property.propertyPath.StartsWith( parentPath ) ) break;    // Don't go into children of other classes

                var item = ctx._displayedMembers.FirstOrDefault( d =>
                    d.memberInfo != null &&
                    (
                        // Standard match: Field name or direct property name matches SerializedProperty name
                        d.memberInfo.Name == property.name ||
                        // Backing field match: Member is a Property, and its expected backing field name matches SerializedProperty name
                        (d.memberInfo is PropertyInfo propInfo && $"<{propInfo.Name}>k__BackingField" == property.name)
                    )
                );
                if( item == null )
                {
                    if( property.name == "m_Script" )   // Special case - m_Script - insert it as 1st element in list
                    {
                        var mScript = new ClassItem() { serializedProperty = property.Copy() };
                        ctx._displayedMembers.Insert( 0, mScript );
                    } else
                    {
                        var isHidden = PropertyUtility.GetAttribute<HideAttribute>( property ) != null
                            || PropertyUtility.GetPropertyType( property ).GetCustomAttribute<HideAttribute>() != null;

                        if( !isHidden ) {
                            // Property was not hidden, so it's strange that we don't have ClassItem for it
                            Debug.LogError( $"Could not find ClassItem for serialized property {property.name} in {ctx._targetObject}!" );
                        }

                        //ctx.members.Add( new ClassItem() { serializedProperty = property.Copy() } );
                    }
                    continue;
                }

                item.serializedProperty = property.Copy();

                // Check if the property has children, and needs to be recursively connected
                if( property.hasChildren && property.isExpanded )
                {
                    if( item.classContext != null )
                    {
                        ConnectWithSerializedProperties( item.classContext, property.Copy() );
                    } else {
                        Debug.LogWarning( $"SerializedProperty {property.name} has children, but '{item.memberInfo.Name}' has no class context!" );
                    }
                }

            } while( property.NextVisible( false ) );
        }


        internal void Draw()
        {
            var oldIndent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = _indentLevel;

            GroupInfo[] lastOpenGroups = null;
            var hiddenGroups = new List<GroupInfo>();

            #region SetActiveGroups()
            // Returns true, if we should display this group or not
            bool SetActiveGroups( GroupInfo[] newOpenedGroups )
            {
                // if( newOpenedGroups == null ) Debug.Log( ">>>>>> SetActiveGroups: NONE" );
                // else Debug.Log( ">>>>>> SetActiveGroups: " + string.Join( ", ", newOpenedGroups.Select( g => g.groupName ) ) );
                
                if( newOpenedGroups != null ) foreach( var g in newOpenedGroups ) {
                    if( hiddenGroups.Contains(g) ) {
                        return false;
                    }
                }

                // 1st: close old groups which need closing
                if( lastOpenGroups != null )
                {
                    for( int i = lastOpenGroups.Length - 1; i >= 0; --i )
                    {
                        var shouldClose = newOpenedGroups == null
                            || i >= newOpenedGroups.Length
                            || lastOpenGroups[i] != newOpenedGroups[i];

                        // We finished with the group - close it
                        if( shouldClose )
                        {
                            var isHidden = false;
                            for( int j = 0; j < i; j++ )
                            {
                                if( hiddenGroups.Contains(lastOpenGroups[j]) )
                                {
                                    isHidden = true;
                                    break;
                                }
                            } 
                            //if( hiddenGroups.Contains( lastOpenGroups[i] ) )
                            if( isHidden )
                            {
                                hiddenGroups.Remove( lastOpenGroups[i] );   // Remove from list of hidden groups
                            }
                            else
                            {
                                lastOpenGroups[i].groupAttribute?.FinishDrawingGroup();
                                //Debug.Log($">> Finish: {lastOpenGroups[i].groupName}");
                            }
                        }
                    }
                }

                // 2nd: Open newly appearing groups
                if( newOpenedGroups != null )
                {
                    for( int i = 0; i < newOpenedGroups.Length; i++ )
                    {
                        var shouldOpen = lastOpenGroups == null
                            || i >= lastOpenGroups.Length
                            || newOpenedGroups[i] != lastOpenGroups[i];

                        if( shouldOpen && newOpenedGroups[i].groupAttribute != null )
                        {
                            var shouldDraw = newOpenedGroups[i].groupAttribute.StartDrawingGroup();
                            //Debug.Log($">> StartGroup: {newOpenedGroups[i].groupName}: draw:{shouldDraw}");
                            if( !shouldDraw )
                            {
                                hiddenGroups.Add( newOpenedGroups[i] );
                                lastOpenGroups = newOpenedGroups;
                                return false;
                            }
                        }
                    }
                }

                lastOpenGroups = newOpenedGroups;
                return true;
            }
            #endregion SetActiveGroups()

            foreach( var item in _displayedMembers )
            {
                // Check which groups have opened and which have closed
                var shouldDraw = SetActiveGroups( item.group?.groups );

                // If item is in a hidden group, don't show it
                if( !shouldDraw ) continue;

                DrawItem( item );
            }

            // Make sure all groups are properly closed
            SetActiveGroups( null );

            EditorGUI.indentLevel = oldIndent;
        }

        private void DrawItem( ClassItem item )
        {
            var itemRect = EditorGUILayout.BeginVertical();

            // Non-serialized properties will have slightly reddish background
            if( item.serializedProperty == null && !(item.memberInfo is MethodInfo) ) {
                GUIUtil.FillRect( itemRect, BgNonSerialized );
            }

            // Draw error message, if any
            if( item.errorMessage != null )
            {
                NiceEditorGUI.HelpBox_Layout( item.errorMessage, MessageType.Error );
                //EditorGUILayout.HelpBox( item.errorMessage, MessageType.Error );
            }

            List<string> preDrawList = null, postDrawList = null;
            if(item.serializedProperty == null && item.niceAttributes.Length > 0)
            {
                foreach( var niceAttribute in item.niceAttributes )
                {
                    if (niceAttribute is OnGUIAttribute onGuiAttribute)
                    {
                        if( !string.IsNullOrWhiteSpace(onGuiAttribute.PreDrawMethodName) )
                        {
                            preDrawList ??= new List<string>();
                            preDrawList.Add( onGuiAttribute.PreDrawMethodName );
                        }
                        if( !string.IsNullOrWhiteSpace(onGuiAttribute.PostDrawMethodName) )
                        {
                            postDrawList ??= new List<string>();
                            postDrawList.Add( onGuiAttribute.PostDrawMethodName );
                        }
                    }
                }
            }

            if (preDrawList != null)
            {
                // Call pre-draw methods
                foreach (var methodName in preDrawList)
                {
                    OnGUIPropertyDrawer.RunGUIMethod(_targetObject, methodName);
                }
            }
            
            
            if( item.classContext != null )                 // *** Class or Struct with expanded view
            {
                if( item.serializedProperty != null ) {         // Draw class foldout
                    EditorGUILayout.PropertyField( item.serializedProperty, false );
                    item.foldedOut = item.serializedProperty.isExpanded;
                } else {
                    item.foldedOut = EditorGUILayout.Foldout( item.foldedOut, ObjectNames.NicifyVariableName( item.memberInfo.Name ), true );
                }

                // Draw only when expanded
                if( item.foldedOut ) {
                    item.classContext.Draw();
                }

            } else if( item.serializedProperty != null )    // *** 'Normal' serialized property
            {
                // Script name - special case
                if( item.serializedProperty.name == "m_Script" )
                {
                    using( new EditorGUI.DisabledScope( disabled: true ) )
                    {
                        EditorGUILayout.PropertyField( item.serializedProperty );
                    }
                } else
                {
                    // Normal, serialized members
                    NiceEditorGUI.PropertyField_Layout( item.serializedProperty, includeChildren: true );
                }
            } else                                          // *** Non-serialized property
            {
                // Non-serialized members
                if( item.memberInfo is FieldInfo field )
                {
                    NiceEditorGUI.NonSerializedField_Layout( _targetObject, field );
                } else if( item.memberInfo is PropertyInfo property )
                {
                    NiceEditorGUI.NativeProperty_Layout( _targetObject, property );
                } else if( item.memberInfo is MethodInfo method )
                {
                    NiceEditorGUI.Button( _targetObject, method );
                }
            }

            if (postDrawList != null)
            {
                // Call post-draw methods
                foreach (var methodName in postDrawList)
                {
                    OnGUIPropertyDrawer.RunGUIMethod(_targetObject, methodName);
                }
            }
            
            EditorGUILayout.EndVertical();
        }
    }
}
