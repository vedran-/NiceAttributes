﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace NiceAttributes.Editor
{
    #region class FormulaParser
    public class FormulaParser
    {
        private readonly object _context;
        private readonly Dictionary<string, Func<object>> _variables = new();
        private readonly Dictionary<string, MethodInfo> _methods = new();

        public FormulaParser( object context )
        {
            _context = context;
            CacheVariables();
            CacheMethods();
        }

        #region CacheVariables()
        private void CacheVariables()
        {
            // Cache fields
            FieldInfo[] fields = _context.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance);
            foreach( var field in fields )
            {
                _variables[field.Name] = () => field.GetValue( _context );
            }

            // Cache properties
            PropertyInfo[] properties = _context.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach( var property in properties )
            {
                _variables[property.Name] = () => property.GetValue( _context );
            }
        }
        #endregion CacheVariables()

        #region CacheMethods()
        private void CacheMethods()
        {
            // Cache methods
            MethodInfo[] methods = _context.GetType().GetMethods(BindingFlags.Public | BindingFlags.Instance);
            foreach( var method in methods )
            {
                if( method.GetParameters().Length == 0 && method.ReturnType == typeof( float ) ) // Only parameterless methods returning float
                {
                    _methods[method.Name] = method;
                }
            }
        }
        #endregion CacheMethods()

        #region Evaluate()
        public float Evaluate( string formula )
        {
            // Tokenize the input string.
            var tokens = Regex.Split( formula, @"([*\/\+\-])" )
                .Where( t => !string.IsNullOrEmpty(t) )
                .Select( t => t.Trim() );

            // Convert infix expression to postfix notation using the Shunting Yard algorithm.
            var outputQueue = ConvertToPostfix(tokens);

            // Evaluate the postfix expression.
            return EvaluatePostfix( outputQueue );
        }
        #endregion Evaluate()

        #region ConvertToPostfix()
        private Queue<string> ConvertToPostfix( IEnumerable<string> tokens )
        {
            var outputQueue = new Queue<string>();
            var operatorStack = new Stack<string>();
            var operators = new Dictionary<string, int> { { "+", 1 }, { "-", 1 }, { "*", 2 }, { "/", 2 } };

            foreach( var token in tokens )
            {
                if( float.TryParse( token, out float number ) )
                {
                    outputQueue.Enqueue( token );
                } else if( _variables.ContainsKey( token ) || _methods.ContainsKey( token ) )
                {
                    outputQueue.Enqueue( token );
                } else if( operators.ContainsKey( token ) )
                {
                    while( operatorStack.Count > 0 && operators.ContainsKey( operatorStack.Peek() ) &&
                           operators[token] <= operators[operatorStack.Peek()] )
                    {
                        outputQueue.Enqueue( operatorStack.Pop() );
                    }
                    operatorStack.Push( token );
                } else
                {
                    throw new ArgumentException( $"Invalid token: {token}" );
                }
            }

            while( operatorStack.Count > 0 )
            {
                outputQueue.Enqueue( operatorStack.Pop() );
            }

            return outputQueue;
        }
        #endregion ConvertToPostfix()

        #region EvaluatePostfix()
        private float EvaluatePostfix( Queue<string> tokens )
        {
            var stack = new Stack<float>();

            while( tokens.Count > 0 )
            {
                var token = tokens.Dequeue();
                if( float.TryParse( token, out float number ) )
                {
                    stack.Push( number );
                } else if( _variables.TryGetValue( token, out Func<object> getter ) )
                {
                    stack.Push( Convert.ToSingle( getter() ) );
                } else if( _methods.TryGetValue( token, out MethodInfo method ) )
                {
                    stack.Push( (float)method.Invoke( _context, null ) );
                } else
                {
                    var rightOperand = stack.Pop();
                    var leftOperand = stack.Pop();

                    switch( token )
                    {
                        case "+":
                            stack.Push( leftOperand + rightOperand );
                            break;
                        case "-":
                            stack.Push( leftOperand - rightOperand );
                            break;
                        case "*":
                            stack.Push( leftOperand * rightOperand );
                            break;
                        case "/":
                            stack.Push( leftOperand / rightOperand );
                            break;
                        default:
                            throw new ArgumentException( $"Unexpected operator: {token}" );
                    }
                }
            }

            if( stack.Count != 1 )
            {
                throw new ArgumentException( "The expression is invalid." );
            }

            return stack.Pop();
        }
        #endregion EvaluatePostfix()
    }
    #endregion class FormulaParser


    #region static class NiceExtension
    public static class NiceExtension
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
    #endregion static class NiceExtension


    [CanEditMultipleObjects]
    [CustomEditor(typeof(UnityEngine.Object), true)]
    public class NiceInspector : UnityEditor.Editor
    {
        ClassContext    rootClass;

        #region OnEnable()
        protected virtual void OnEnable()
        {
            rootClass = ClassContext.CreateContext( target.GetType(), serializedObject.targetObject, 0 );
            if( rootClass.hasNiceAttributes ) {
                ClassContext.ConnectWithSerializedProperties( rootClass, serializedObject.GetIterator() );
            }
        }
        #endregion OnEnable()

        #region OnDisable()
        protected virtual void OnDisable()
        {
            ReorderableListPropertyDrawer.Instance.ClearCache();
        }
        #endregion OnDisable()


        #region OnInspectorGUI()
        public override void OnInspectorGUI()
        {
            if( rootClass == null || !rootClass.hasNiceAttributes )
            {
                DrawDefaultInspector();
                return;
            }

            // Draw our custom Inspector
            serializedObject.Update();

            rootClass.Draw();

            serializedObject.ApplyModifiedProperties();
        }
        #endregion OnInspectorGUI()
    }
}
