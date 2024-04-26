using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace NiceAttributes.Editor
{
    public class ClassContext
    {
        public bool             HasNiceAttributes { get; private set; } = false;

        private List<ClassItem> members = new();
        private object          targetObject;
        private int             indentLevel;


        #region class ClassItem
        public class ClassItem
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
        #endregion class ClassItem

        #region class GroupInfo
        public class GroupInfo
        {
            public string               groupName;
            public BaseGroupAttribute   groupAttribute = null;
            public GroupInfo[]          groups; // List of all groups this group belongs to - including itself
            internal TabGroupAttribute.TabParent tabParent;

            public GroupInfo( string name ) { groupName = name; }

            internal GroupInfo GetParentGroup()
            {
                if( groups.Length < 2 ) return null;
                return groups[groups.Length - 2];
            }
        }
        #endregion class GroupInfo

        // Private constructor - call CreateContext() to create a new instance
        private ClassContext() {}


        #region [API] CreateContext()
        /// <summary>
        /// Get all members of the class and all its base classes.
        /// It also recursively visits subclasses/substructs
        /// </summary>
        /// <param name="classType"></param>
        /// <param name="targetObject"></param>
        /// <param name="indentLevel"></param>
        /// <param name="additionalSkipTypes"></param>
        public static ClassContext CreateContext(Type classType, object targetObject, int indentLevel,
            Type[] additionalSkipTypes = null)
        {
            var ctx = new ClassContext();
            ctx.targetObject = targetObject;
            ctx.indentLevel = indentLevel;

            // Get all class members
            // NOTE: it can call CreateContext() on any field which we want do display expanded (e.g. non-serialized class with [Show] attribute)
            ctx.GetAllMembers( classType, additionalSkipTypes );

            //Debug.Log( $"All members {ctx.hasNiceAttributes} for {classType.Name}:\n - " + string.Join( "\n - ", ctx.members.Select( m => $"{m.memberInfo} - {m.memberInfo.GetInterfaceAttributes<INiceAttribute>().FirstOrDefault()?.LineNumber} >> [{ string.Join( ", ", m.niceAttributes.Select( a => a.GetType() ) )}]" ) ) );

            // If class (or its subclasses) don't have any INiceAttribute, stop further processing
            // - we'll then use default inspector instead of our anyhow
            if( !ctx.HasNiceAttributes ) return ctx;

            // Order class members, as they appear in the source code file
            var orderedMembers = ctx.GetOrderedMembersByLineNumber();

            ctx.BuildDisplayTree( orderedMembers );

            ctx.InitializeTabs();

            return ctx;
        }
        #endregion CreateContext()


        #region IsVisible()
        public bool IsVisible( MemberInfo mt )
        {
            if( Attribute.IsDefined( mt, typeof( HideAttribute ) ) )
            {
                HasNiceAttributes = true;
                return false;
            }

            if( mt.MemberType == MemberTypes.Property ) return Attribute.IsDefined( mt, typeof( ShowAttribute ) );
            if( mt.MemberType == MemberTypes.Method ) return Attribute.IsDefined( mt, typeof( ButtonAttribute ) );
            if( mt.MemberType != MemberTypes.Field ) return false;

            // Field
            var fi = mt as FieldInfo;
            if( Attribute.IsDefined( fi, typeof( ShowAttribute ) ) ) return true;
            if( fi.IsStatic ) return false;     // Static fields - not visible

            var isVisible = (fi.IsPublic && !Attribute.IsDefined( fi, typeof( NonSerializedAttribute ) ))
                || Attribute.IsDefined( fi, typeof( SerializeField ) );

            var fieldType = fi.FieldType;
            if( isVisible )     // Check if basic field type is class or struct
            {
                if( fieldType.IsArray ) fieldType = fieldType.GetElementType();
                if( fieldType.IsGenericList() ) fieldType = fieldType.GetGenericArguments()[0];

                // Class or struct - only if it has [Serialized] attribute, or if it inherits from ScriptableObject
                if( isVisible && fieldType.IsClassOrStruct() )
                {
                    isVisible = Attribute.IsDefined( fieldType, typeof( SerializableAttribute ) )
                        || Attribute.IsDefined( fieldType, typeof( ShowAttribute ) )
                        || fieldType.IsSubclassOfRawGeneric( typeof( UnityEngine.Object ) );
                }
            }
            return isVisible;
        }
        #endregion IsVisible()

        #region GetAllMembers()
        const BindingFlags  AllBindingFields = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.DeclaredOnly;
        readonly static Type[] SkipTypes = new Type[] { typeof(UnityEngine.Object), typeof(ScriptableObject), typeof(MonoBehaviour) };
        void GetAllMembers( Type classType, Type[] additionalSkipTypes )
        {
            // Add all base types to a list - we need their members too, we inherited them!
            var types = new List<Type>() { classType };
            while( types.Last().BaseType != null )
            {
                var bt = types.Last().BaseType;
                if( SkipTypes.Contains( bt ) ) break;
                if( additionalSkipTypes != null && additionalSkipTypes.Contains( bt ) ) break;
                types.Add( bt );
            }

            void AddMember( MemberInfo member, ClassContext memberClassContext )
            {
                var n = new ClassItem() {
                    memberInfo = member,
                    niceAttributes = member.GetInterfaceAttributes<INiceAttribute>().ToArray(),
                    classContext = memberClassContext
                };
                if( n.niceAttributes.Length > 0 )
                {
                    HasNiceAttributes = true;
                }

                // Get line number in source code from INiceAttribute
                n.lineNumber = n.niceAttributes.Length > 0 ? n.niceAttributes[0].LineNumber : -1f;

                members.Add( n );
            }


            // Get all members of current class + all base classes
            for( int i = types.Count - 1; i >= 0; i-- )
            {
                var members = types[i]
                    .GetMembers( AllBindingFields )
                    .Where( m => IsVisible( m ) );      // Only visible members!

                foreach( var m in members )
                {
                    // Check member type
                    var fieldInfo = m as FieldInfo;
                    var propInfo = m as PropertyInfo;
                    var methodInfo = m as MethodInfo;
                    var memberType = fieldInfo != null ? fieldInfo.FieldType
                        : propInfo != null ? propInfo.PropertyType
                        : methodInfo != null ? methodInfo.ReturnType
                        : null;

                    ClassContext memberClassContext = null;
                    if( memberType != null && !memberType.IsArray && !memberType.IsGenericList() )
                    {
                        //if( memberType.IsArray ) memberType = memberType.GetElementType();
                        //if( memberType.IsGenericList() ) memberType = memberType.GetGenericArguments()[0];

                        var treatAsClassOrStruct = memberType.IsClassOrStruct()
                                                   && memberType != typeof(string);
                        
                        if( treatAsClassOrStruct )  // Class or structs - check if we want to show them expanded (with all their members)
                        {
                            // Class or Struct has [Hide] attribute - so don't show this whole field at all
                            if( Attribute.IsDefined( memberType, typeof( HideAttribute ) ) ) {
                                HasNiceAttributes = true;
                                continue;
                            }

                            var useExpandedView = 
                                (Attribute.IsDefined( memberType, typeof( SerializableAttribute ) ) || Attribute.IsDefined( memberType, typeof( ShowAttribute ) ))
                                && !memberType.IsSubclassOfRawGeneric( typeof( UnityEngine.Object ) );

                            // Show expanded view of the class/struct - get all its members in a new class context!
                            if( useExpandedView )
                            {
                                var obj = propInfo != null ? propInfo.GetValue( targetObject, null )
                                    : fieldInfo != null ? fieldInfo.GetValue( targetObject )
                                    : null;
                                Debug.Assert( obj != null, $"Object for member {m.Name} is null! Parent object was {targetObject}" );

                                // Create new context for the sub-class
                                memberClassContext = CreateContext( memberType, obj, indentLevel + 1 );

                                // If sub-class has any Nice attributes, then mark that we have nice attributes
                                if( memberClassContext.HasNiceAttributes ) HasNiceAttributes = true;
                            }
                        }
                    }

                    AddMember( m, memberClassContext );
                }
            }
        }
        #endregion GetAllMembers()

        #region GetOrderedMembersByLineNumber()
        IEnumerable<ClassItem> GetOrderedMembersByLineNumber()
        {
            // For members which don't have line numbers, select a number between their neighbours which do have line numbers
            // But make sure that only use neighbours of the same type!
            MemberTypes lastType = 0;
            float minValue = -1;
            for( int idx = 0; idx < members.Count; idx++ )
            {
                var m = members[idx];
                if( m.lineNumber < 0 )
                {
                    // If member type changed from last member, then reset last min value
                    if( m.memberInfo.MemberType != lastType ) minValue = -1;

                    // Find next member of the same type, which has line number set.
                    var maxValue = -1f;
                    for( int i = idx + 1; i < members.Count; i++ ) {
                        if( members[i].memberInfo.MemberType != m.memberInfo.MemberType ) break;   // Not in our group any more, stop the search
                        if( members[i].lineNumber < 0 ) continue;
                        maxValue = members[i].lineNumber;
                        break;
                    }

                    float GetNewLineNumber()
                    {
                        // If no memeber of this type has line number set, return number
                        if( minValue < 0 && maxValue < 0 ) return (int)m.memberInfo.MemberType * 10f;
                        if( minValue < 0 && maxValue >= 0 ) return maxValue - 1f;
                        if( maxValue < 0 && minValue >= 0 ) return minValue + 1f;
                        return (minValue + maxValue) / 2f;
                    }

                    // Set new calculated line number - this will be used for sorting
                    m.lineNumber = GetNewLineNumber();
                }

                lastType = m.memberInfo.MemberType;
                minValue = m.lineNumber;
            }

            // Finally, order all members by line numbers
            return members.OrderBy( m => m.lineNumber );
        }
        #endregion GetOrderedMembersByLineNumber()

        #region BuildDisplayTree()
        void BuildDisplayTree( IEnumerable<ClassItem> orderedMembers )
        {
            var groups = new Dictionary<string, GroupInfo>();

            // Create root group
            var rootGroup = new GroupInfo("root");
            groups["root"] = rootGroup;
            rootGroup.groups = new GroupInfo[] { rootGroup };

            #region GetGroup()
            GroupInfo GetGroup( string groupName )
            {
                // Check if group already exists
                if( groups.TryGetValue( groupName, out var groupInfo ) ) return groupInfo;

                // Group does not exist - create new entry - or entries
                var gs = groupName.Split( '/', StringSplitOptions.None );

                var sb = new System.Text.StringBuilder();
                var allGroups = new List<GroupInfo>();
                foreach( var name in gs )
                {
                    // Add a subgroup name to end of string
                    if( sb.Length > 0 ) sb.Append( '/' );
                    sb.Append( name );
                    var gname = sb.ToString();

                    if( !groups.TryGetValue( gname, out var gi ) )
                    {
                        // This group does not exist yet - so create it!
                        var n = new GroupInfo(gname);
                        groups[gname] = n;
                        allGroups.Add( n );
                        n.groups = allGroups.ToArray();
                    } else allGroups.Add( gi );
                }

                return allGroups.Last();
            }
            #endregion GetGroup()


            // Create list of all visible members, and order them by groups they belong to
            var items = new List<ClassItem>();
            foreach( var member in orderedMembers )
            {
                #region Check all groups it belongs to
                var errors = new List<string>();
                int deepestLevel = -1;
                GroupInfo mainGroupInfo = null;
                var groupAttributes = member.memberInfo.GetCustomAttributes<BaseGroupAttribute>();
                foreach( var groupAttribute in groupAttributes )
                {
                    var groupName = groupAttribute != null ? $"root/{groupAttribute.GroupName}" : "root";
                    var groupInfo = GetGroup( groupName );

                    // First direct assignment of this group type
                    // That way, we can use groups with subgroups, even before we define them!
                    if( groupInfo.groupAttribute == null ) groupInfo.groupAttribute = groupAttribute;

                    if( groupInfo.groups != null && groupInfo.groups.Length > deepestLevel )
                    {
                        deepestLevel = groupInfo.groups.Length;
                        mainGroupInfo = groupInfo;
                    }

                    // TODO: Check if mainGroupInfo and groupInfo are on the same branch of the tree!
                    // TODO2: If not, perhaps display the item on 2 (or more) positions?

                    // Make sure that group type hasn't changed!
                    if( groupAttribute != null && groupInfo.groupAttribute.GetType() != groupAttribute.GetType() ) {
                        errors.Add( $"Group type {groupAttribute.GetType().Name} is different from original {groupInfo.groupAttribute.GetType().Name} for group '{groupInfo.groupName}'!" );
                    }
                }
                if( mainGroupInfo == null ) {   // If item doesn't belong to any groups
                    mainGroupInfo = rootGroup;
                }
                #endregion Check all groups it belongs to

                // Create item for it!
                member.group = mainGroupInfo;
                member.errorMessage = errors.Count > 0 ? string.Join( "\n", errors ) : null;


                // Here is where the magic happens:
                // - it sorts all the items by the groups, but it tries to keep the original order of all the members

                // Find last item, which has the most same groups as we
                // - this will be our insertion point!
                int mostGroupsIdx = -1, mostGroupsCount = -1;
                for( int idx = 0; idx < items.Count; idx++ )
                {
                    int len = Mathf.Min( items[idx].group.groups.Length, mainGroupInfo.groups.Length );
                    for( int j = 0; j < len; j++ )
                    {
                        if( items[idx].group.groups[j] != mainGroupInfo.groups[j] ) break;

                        if( j >= mostGroupsCount )
                        {
                            mostGroupsIdx = idx;
                            mostGroupsCount = j;
                        }
                    }
                }

                if( mostGroupsIdx < 0 ) items.Add( member );     // Insert on last position in the list
                else items.Insert( mostGroupsIdx + 1, member );  // Insert after last item from this group
                //Debug.Log( $"Item order: " + string.Join( ", ", items.Select( i => $"{i.memberInfo.Name} [{i.group.groupName}]" ) ) );
            }

            // Replace old list with a new list, sorted by groups
            this.members = items;
        }
        #endregion BuildDisplayTree()
    
        #region InitializeTabs()
        void InitializeTabs()
        {
            foreach( var item in members )
            {
                if( item.memberInfo == null ) continue;
                if( item.group.groupAttribute is not TabGroupAttribute tabAtt ) continue;

                var parentGroup = item.group.GetParentGroup();
                if( parentGroup == null ) {
                    item.errorMessage = $"TabGroup '{item.group.groupName}' on member '{item.memberInfo.Name}' has no parent!";
                    continue;
                }

                if( parentGroup.tabParent == null )
                {
                    // New Tab Group
                    parentGroup.tabParent = new TabGroupAttribute.TabParent() {
                        tabGroups = new List<TabGroupAttribute>() { tabAtt }
                    };
                } else {
                    if( !parentGroup.tabParent.tabGroups.Contains( tabAtt ) ) parentGroup.tabParent.tabGroups.Add( tabAtt );
                }

                tabAtt.tabParent = parentGroup.tabParent;
            }

            // TODO: Reorder members, so all with same Tab parent are next to each other
            // If not, whole Tab Group can jump from place to place when user switches tabs
        }
        #endregion InitializeTabs()

        #region [API] ConnectWithSerializedProperties()
        internal static void ConnectWithSerializedProperties( ClassContext ctx, SerializedProperty property )
        {
            var parentPath = property.propertyPath;
            if( !property.NextVisible( true ) ) return;

            do
            {
                if( !property.propertyPath.StartsWith( parentPath ) ) break;    // Don't go into children of other classes

                var item = ctx.members.FirstOrDefault( d => d.memberInfo != null && d.memberInfo.Name == property.name );
                if( item == null )
                {
                    if( property.name == "m_Script" )   // Special case - m_Script - insert it as 1st element in list
                    {
                        var m_Script = new ClassItem() { serializedProperty = property.Copy() };
                        ctx.members.Insert( 0, m_Script );
                    } else
                    {
                        var isHidden = PropertyUtility.GetAttribute<HideAttribute>( property ) != null
                            || PropertyUtility.GetPropertyType( property ).GetCustomAttribute<HideAttribute>() != null;

                        if( !isHidden ) {
                            // Property was not hidden, so it's strange that we don't have ClassItem for it
                            Debug.LogError( $"Could not find ClassItem for serialized property {property.name} in {ctx.targetObject}!" );
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
        #endregion ConnectWithSerializedProperties()


        #region [API] Draw()
        internal void Draw()
        {
            var oldIndent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = indentLevel;

            GroupInfo[] lastOpenGroups = null;
            var hiddenGroups = new List<GroupInfo>();

            #region SetActiveGroups()
            // Returns true, if we should display this group or not
            bool SetActiveGroups( GroupInfo[] newOpenedGroups )
            {
                if( newOpenedGroups != null ) foreach( var g in newOpenedGroups ) if( hiddenGroups.Contains( g ) ) return false;

                // 1st: close old groups which need closing
                if( lastOpenGroups != null )
                {
                    for( int i = lastOpenGroups.Length - 1; i >= 0; --i )
                    {
                        var shouldClose = newOpenedGroups == null
                        || i >= newOpenedGroups.Length
                        || lastOpenGroups[i] != newOpenedGroups[i];

                        if( shouldClose ) {     // We finished with the group
                            if( hiddenGroups.Contains( lastOpenGroups[i] ) )
                            {
                                hiddenGroups.Remove( lastOpenGroups[i] );   // Remove from list of hidden groups
                            } else lastOpenGroups[i].groupAttribute?.OnGUI_GroupEnd();
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

                        if( shouldOpen )
                        {
                            if( newOpenedGroups[i].groupAttribute != null )
                            {
                                var shouldDraw = newOpenedGroups[i].groupAttribute.OnGUI_GroupStart();
                                if( !shouldDraw )
                                {
                                    hiddenGroups.Add( newOpenedGroups[i] );
                                    lastOpenGroups = newOpenedGroups;
                                    return false;
                                }
                            }
                        }
                    }
                }

                lastOpenGroups = newOpenedGroups;
                return true;
            }
            #endregion SetActiveGroups()

            foreach( var item in members )
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
        #endregion Draw()


        #region DrawItem()
        static readonly Color bgNonSerialized = Color.Lerp( DrawingUtil.GetDefaultBackgroundColor(), new Color32(127, 0, 0, 255), 0.15f );
        private void DrawItem( ClassItem item )
        {
            var itemRect = EditorGUILayout.BeginVertical();

            // Non-serialized properties will have slightly reddish background
            if( item.serializedProperty == null && !(item.memberInfo is MethodInfo) ) {
                DrawingUtil.FillRect( itemRect, bgNonSerialized );
            }

            // Draw error message, if any
            if( item.errorMessage != null )
            {
                NiceEditorGUI.HelpBox_Layout( item.errorMessage, MessageType.Error );
                //EditorGUILayout.HelpBox( item.errorMessage, MessageType.Error );
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
                    NiceEditorGUI.NonSerializedField_Layout( targetObject, field );
                } else if( item.memberInfo is PropertyInfo property )
                {
                    NiceEditorGUI.NativeProperty_Layout( targetObject, property );
                } else if( item.memberInfo is MethodInfo method )
                {
                    NiceEditorGUI.Button( targetObject, method );
                }
            }

            EditorGUILayout.EndVertical();
        }
        #endregion DrawItem()
    }
}
