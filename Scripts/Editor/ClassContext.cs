using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using NiceAttributes.Editor.Discovery;
using NiceAttributes.Editor.Grouping;
using NiceAttributes.Editor.Ordering;
using NiceAttributes.Editor.PropertyDrawers;
using NiceAttributes.Editor.Rendering;
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
        
        
        public bool             HasNiceAttributes { get; private set; } = false;

        internal List<ClassItem> _displayedMembers = null;
        internal object          _targetObject;
        internal int             _indentLevel;


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

        
        internal void Draw(object targetForGroups = null)
        {
            // Use provided target (for nested contexts), or fall back to own target
            var effectiveTarget = targetForGroups ?? _targetObject;
            var renderer = new ClassRenderer(_displayedMembers, _targetObject, _indentLevel, effectiveTarget);
            renderer.Render();
        }
    }
}
