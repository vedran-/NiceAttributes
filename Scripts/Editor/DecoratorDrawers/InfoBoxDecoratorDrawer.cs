using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using NiceAttributes.Editor.Utility;
using UnityEditor;
using UnityEngine;

namespace NiceAttributes.Editor.DecoratorDrawers
{
    [CustomPropertyDrawer( typeof( InfoBoxAttribute ) )]
    public class InfoBoxDecoratorDrawer : DecoratorDrawer
    {
        private static object _currentTarget;
        private static SerializedProperty _currentProperty;
        #region GetHeight()
        public override float GetHeight()
        {
            float minHeight = EditorGUIUtility.singleLineHeight * GUIConstants.InfoBoxHeightMultiplier;
            var (content, hasError) = GetTextContent();
            float desiredHeight = GUI.skin.box.CalcHeight( content, EditorGUIUtility.currentViewWidth );
            float height = Mathf.Max( minHeight, desiredHeight ) + GUIConstants.InfoBoxExtraHeight;
            return height;
        }
        #endregion GetHeight()

        #region OnGUI()
        public override void OnGUI( Rect rect )
        {
            var indentLength = NiceEditorGUI.GetIndentLength(rect);
            var infoBoxRect = new Rect( rect.x + indentLength, rect.y, rect.width - indentLength, GetHeight() );
            var (content, hasError) = GetTextContent();
            //var messageType = ConvertInfoBoxType( (attribute as InfoBoxAttribute).Type );
            var messageType = hasError ? MessageType.Error : ConvertInfoBoxType( (attribute as InfoBoxAttribute).Type );

            NiceEditorGUI.HelpBox( infoBoxRect, content.text, messageType );
        }
        #endregion OnGUI()

        #region [Util] ConvertInfoBoxType()
        private MessageType ConvertInfoBoxType( InfoBoxType type )
        {
            switch( type )
            {
                case InfoBoxType.Info: return MessageType.Info;
                case InfoBoxType.Warning: return MessageType.Warning;
                case InfoBoxType.Error: return MessageType.Error;
                default: throw new System.Exception( $"Invalid type {type}" );
            }
        }
        #endregion ConvertInfoBoxType()


        #region [Util] GetTextContent()
        private (GUIContent content, bool hasError) GetTextContent()
        {
            var infoBoxAttribute = attribute as InfoBoxAttribute;
            if( infoBoxAttribute == null ) return (GUIContent.none, false);

            bool hasLookupError = false;
            var parseResult = MathematicalParser.Evaluate( infoBoxAttribute.Text, GetVariableValue, GetFunctionValue );
            var text = parseResult.result?.ToString() ?? string.Empty;
            bool hasError = hasLookupError || !parseResult.successful;

            return (new GUIContent( text ), hasError);
        }
        #endregion GetTextContent()


        private object GetVariableValue( string variableName )
        {
            try
            {
                object target = GetTargetObject();
                if( target == null ) return CreateErrorResult( "Could not resolve target object." );

                FieldInfo fieldInfo = ReflectionUtility.GetField( target, variableName );
                if( fieldInfo != null ) return fieldInfo.GetValue( target );

                PropertyInfo propertyInfo = ReflectionUtility.GetProperty( target, variableName );
                if( propertyInfo != null ) return propertyInfo.GetValue( target );

                MethodInfo methodInfo = ReflectionUtility.GetMethod( target, variableName );
                if( methodInfo != null )
                {
                    if( methodInfo.ReturnType == typeof( void ) || methodInfo.GetParameters().Length != 0 )
                    {
                        return CreateErrorResult( $"Member '{variableName}' must be a field, property, or parameterless method." );
                    }

                    return methodInfo.Invoke( target, null );
                }

                return CreateErrorResult( $"Member '{variableName}' was not found." );
            }
            catch( Exception ex )
            {
                return CreateErrorResult( GetExceptionMessage( ex ) );
            }
        }

        private object GetFunctionValue( string functionName, object parameters )
        {
            try
            {
                object target = GetTargetObject();
                if( target == null ) return CreateErrorResult( "Could not resolve target object." );

                object[] parameterValues = GetParameterValues( parameters );
                MethodInfo methodInfo = GetMethod( target, functionName, parameterValues, out object[] convertedParameters );
                if( methodInfo == null )
                {
                    return CreateErrorResult( $"Function '{functionName}' was not found or parameters are invalid." );
                }

                return methodInfo.Invoke( target, convertedParameters );
            }
            catch( Exception ex )
            {
                return CreateErrorResult( GetExceptionMessage( ex ) );
            }
        }

        private object GetTargetObject()
        {
            return PropertyDrawPipeline.CurrentTarget;
        }

        private MethodInfo GetMethod( object target, string functionName, object[] parameterValues, out object[] convertedParameters )
        {
            convertedParameters = null;

            MethodInfo methodInfo = ReflectionUtility.GetMethod( target, functionName );
            if( TryPrepareMethodParameters( methodInfo, parameterValues, out convertedParameters ) ) return methodInfo;

            foreach( MethodInfo candidate in ReflectionUtility.GetAllMethods( target, m => m.Name.Equals( functionName, StringComparison.Ordinal ) ) )
            {
                if( TryPrepareMethodParameters( candidate, parameterValues, out convertedParameters ) ) return candidate;
            }

            return null;
        }

        private object[] GetParameterValues( object parameters )
        {
            if( parameters is List<object> parameterList ) return parameterList.ToArray();
            if( parameters == null ) return new object[0];
            return new[] { parameters };
        }

        private bool TryPrepareMethodParameters( MethodInfo methodInfo, object[] parameterValues, out object[] convertedParameters )
        {
            convertedParameters = null;
            if( methodInfo == null ) return false;

            ParameterInfo[] parameterInfos = methodInfo.GetParameters();
            if( parameterInfos.Length != parameterValues.Length ) return false;

            convertedParameters = new object[parameterInfos.Length];
            for( int i = 0; i < parameterInfos.Length; i++ )
            {
                if( !TryConvertValue( parameterValues[i], parameterInfos[i].ParameterType, out convertedParameters[i] ) ) return false;
            }

            return true;
        }

        private bool TryConvertValue( object value, Type targetType, out object convertedValue )
        {
            convertedValue = null;

            Type underlyingType = Nullable.GetUnderlyingType( targetType ) ?? targetType;
            if( value == null )
            {
                if( underlyingType.IsValueType && Nullable.GetUnderlyingType( targetType ) == null ) return false;
                return true;
            }

            if( underlyingType.IsInstanceOfType( value ) )
            {
                convertedValue = value;
                return true;
            }

            try
            {
                if( underlyingType.IsEnum )
                {
                    if( value is string enumString )
                    {
                        convertedValue = Enum.Parse( underlyingType, enumString, true );
                        return true;
                    }

                    object enumValue = Convert.ChangeType( value, Enum.GetUnderlyingType( underlyingType ), CultureInfo.InvariantCulture );
                    convertedValue = Enum.ToObject( underlyingType, enumValue );
                    return true;
                }

                convertedValue = Convert.ChangeType( value, underlyingType, CultureInfo.InvariantCulture );
                return true;
            }
            catch
            {
                convertedValue = null;
                return false;
            }
        }

        private object CreateErrorResult( string message )
        {
            return $"ERROR: {message}";
        }

        private string GetExceptionMessage( Exception exception )
        {
            while( exception is TargetInvocationException && exception.InnerException != null )
            {
                exception = exception.InnerException;
            }

            return exception.Message;
        }
    }
}
