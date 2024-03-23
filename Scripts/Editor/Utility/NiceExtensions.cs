using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace NiceAttributes.Editor
{
    public static class NiceExtensions
    {
        #region IsClassOrStruct()
        public static bool IsClassOrStruct( this Type type )
        {
            return (type.IsClass || (type.IsValueType && !type.IsPrimitive)) && !type.IsEnum;
        }
        #endregion IsClassOrStruct()

        #region IsGenericList()
        // From https://stackoverflow.com/a/951602/1111634
        public static bool IsGenericList( this Type type )
        {
            if( type == null )
            {
                Debug.LogError( "Type is null!" );
                return false;
            }

            foreach( Type inter in type.GetInterfaces() )
            {
                if( !inter.IsGenericType ) continue;
                if( inter.GetGenericTypeDefinition() == typeof( ICollection<> ) )
                {
                    // if needed, you can also return the type used as generic argument
                    return true;
                }
            }
            return false;
        }
        #endregion IsGenericList()

        #region IsSubclassOfRawGeneric()
        // From https://stackoverflow.com/a/457708/1111634
        public static bool IsSubclassOfRawGeneric( this Type type, Type genericType )
        {
            var t = type;
            while( t != null && t != typeof( object ) )
            {
                var cur = t.IsGenericType ? t.GetGenericTypeDefinition() : t;
                if( genericType == cur ) return true;

                t = t.BaseType;
            }
            return false;
        }
        #endregion IsSubclassOfRawGeneric()

        #region GetInterfaceAttribute()
        // This extension method retrieves the first custom attribute that implements the interface T
        public static T GetInterfaceAttribute<T>( this MemberInfo member ) where T : class
        {
            // Get all custom attributes and filter by the interface type
            var attribute = member.GetCustomAttributes(true)
                .FirstOrDefault( attr => typeof(T).IsAssignableFrom(attr.GetType()) ) as T;

            return attribute;
        }
        #endregion GetInterfaceAttribute()
        #region GetInterfaceAttributes()
        public static IEnumerable<T> GetInterfaceAttributes<T>( this MemberInfo member ) where T : class
        {
            // Get all custom attributes and filter by the interface type
            return member.GetCustomAttributes( true )
                .Where( attr => typeof(T).IsAssignableFrom(attr.GetType()) )
                .Select( o => o as T );
        }
        #endregion GetInterfaceAttributes()
    }
}