using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace NiceAttributes.Editor
{
    public static class NiceEditorGUI
    {
        public const float IndentLength = 15.0f;
        public const float HorizontalSpacing = 2.0f;

        private static GUIStyle _buttonStyle = new GUIStyle(GUI.skin.button) { richText = true };

        private delegate void PropertyFieldFunction( Rect rect, SerializedProperty property, GUIContent label, bool includeChildren );

        #region PropertyField()
        public static void PropertyField( Rect rect, SerializedProperty property, bool includeChildren )
        {
            PropertyField_Implementation( rect, property, includeChildren, DrawPropertyField );
        }
        #endregion PropertyField()
        #region PropertyField_Layout()
        public static void PropertyField_Layout( SerializedProperty property, bool includeChildren )
        {
            Rect dummyRect = new Rect();
            PropertyField_Implementation( dummyRect, property, includeChildren, DrawPropertyField_Layout );
        }
        #endregion PropertyField_Layout()

        #region DrawPropertyField()
        private static void DrawPropertyField( Rect rect, SerializedProperty property, GUIContent label, bool includeChildren )
        {
            EditorGUI.PropertyField( rect, property, label, includeChildren );
        }
        #endregion DrawPropertyField()
        #region DrawPropertyField_Layout()
        private static void DrawPropertyField_Layout( Rect rect, SerializedProperty property, GUIContent label, bool includeChildren )
        {
            EditorGUILayout.PropertyField( property, label, includeChildren );
        }
        #endregion DrawPropertyField_Layout()

        #region PropertyField_Implementation()
        private static void PropertyField_Implementation( Rect rect, SerializedProperty property, bool includeChildren, PropertyFieldFunction propertyFieldFunction )
        {
            SpecialCaseDrawerAttribute specialCaseAttribute = PropertyUtility.GetAttribute<SpecialCaseDrawerAttribute>(property);
            if( specialCaseAttribute != null )
            {
                specialCaseAttribute.GetDrawer().OnGUI( rect, property );
            } else
            {
                // Check if visible
                bool visible = PropertyUtility.IsVisible(property);
                if( !visible ) {
                    return;
                }

                // Validate
                ValidatorAttribute[] validatorAttributes = PropertyUtility.GetAttributes<ValidatorAttribute>(property);
                foreach( var validatorAttribute in validatorAttributes )
                {
                    validatorAttribute.GetValidator().ValidateProperty( property );
                }

                // Check if enabled and draw
                EditorGUI.BeginChangeCheck();
                bool enabled = PropertyUtility.IsEnabled(property);
                using( new EditorGUI.DisabledScope( disabled: !enabled ) )
                {
                    propertyFieldFunction.Invoke( rect, property, PropertyUtility.GetLabel( property ), includeChildren );
                }

                // Call OnValueChanged callbacks
                if( EditorGUI.EndChangeCheck() )
                {
                    PropertyUtility.CallOnValueChangedCallbacks( property );
                }
            }
        }
        #endregion PropertyField_Implementation()



        #region Dropdown()
        /// <summary>
        /// Creates a dropdown
        /// </summary>
        /// <param name="rect">The rect the defines the position and size of the dropdown in the inspector</param>
        /// <param name="serializedObject">The serialized object that is being updated</param>
        /// <param name="target">The target object that contains the dropdown</param>
        /// <param name="dropdownField">The field of the target object that holds the currently selected dropdown value</param>
        /// <param name="label">The label of the dropdown</param>
        /// <param name="selectedValueIndex">The index of the value from the values array</param>
        /// <param name="values">The values of the dropdown</param>
        /// <param name="displayOptions">The display options for the values</param>
        public static void Dropdown( Rect rect, 
            SerializedObject serializedObject, object target, FieldInfo dropdownField,
            string label, int selectedValueIndex, object[] values, string[] displayOptions )
        {
            EditorGUI.BeginChangeCheck();

            int newIndex = EditorGUI.Popup(rect, label, selectedValueIndex, displayOptions);
            object newValue = values[newIndex];

            object dropdownValue = dropdownField.GetValue(target);
            if( dropdownValue == null || !dropdownValue.Equals( newValue ) )
            {
                Undo.RecordObject( serializedObject.targetObject, "Dropdown" );

                // TODO: Problem with structs, because they are value type.
                // The solution is to make boxing/unboxing but unfortunately I don't know the compile time type of the target object
                dropdownField.SetValue( target, newValue );
            }
        }
        #endregion Dropdown()

        #region Button()
        public static void Button( object target, MethodInfo methodInfo )
        {
            bool visible = ButtonUtility.IsVisible( target, methodInfo );
            if( !visible )
            {
                return;
            }

            if( methodInfo.GetParameters().All( p => p.IsOptional ) )
            {
                ButtonAttribute buttonAttribute = (ButtonAttribute)methodInfo.GetCustomAttributes(typeof(ButtonAttribute), true)[0];
                string buttonText = string.IsNullOrEmpty(buttonAttribute.Text) ? ObjectNames.NicifyVariableName(methodInfo.Name) : buttonAttribute.Text;

                bool buttonEnabled = ButtonUtility.IsEnabled( target, methodInfo );

                ButtonEnableMode mode = buttonAttribute.SelectedEnableMode;
                buttonEnabled &=
                    mode == ButtonEnableMode.Always ||
                    mode == ButtonEnableMode.Editor && !Application.isPlaying ||
                    mode == ButtonEnableMode.Playmode && Application.isPlaying;

                bool methodIsCoroutine = methodInfo.ReturnType == typeof(IEnumerator);
                if( methodIsCoroutine )
                {
                    buttonEnabled &= (Application.isPlaying ? true : false);
                }

                EditorGUI.BeginDisabledGroup( !buttonEnabled );

                if( GUILayout.Button( buttonText, _buttonStyle ) )
                {
                    object[] defaultParams = methodInfo.GetParameters().Select(p => p.DefaultValue).ToArray();
                    IEnumerator methodResult = methodInfo.Invoke(target, defaultParams) as IEnumerator;

                    if( !Application.isPlaying )
                    {
                        // Set target object and scene dirty to serialize changes to disk
                        var obj = target as UnityEngine.Object;
                        if( obj != null ) EditorUtility.SetDirty( obj );

                        PrefabStage stage = PrefabStageUtility.GetCurrentPrefabStage();
                        if( stage != null )
                        {
                            // Prefab mode
                            EditorSceneManager.MarkSceneDirty( stage.scene );
                        } else
                        {
                            // Normal scene
                            EditorSceneManager.MarkSceneDirty( EditorSceneManager.GetActiveScene() );
                        }
                    } else if( methodResult != null && target is MonoBehaviour behaviour )
                    {
                        behaviour.StartCoroutine( methodResult );
                    }
                }

                EditorGUI.EndDisabledGroup();
            } else
            {
                string warning = typeof(ButtonAttribute).Name + $" works only on methods with no parameters [{methodInfo.Name}]";
                HelpBox_Layout( warning, MessageType.Warning, context: target as UnityEngine.Object, logToConsole: true );
            }
        }
        #endregion Button()

        #region NativeProperty_Layout()
        public static void NativeProperty_Layout( object target, PropertyInfo property )
        {
            object value = property.GetValue(target, null);

            if( value == null )
            {
                string warning = string.Format("{0} is null. {1} doesn't support reference types with null value", ObjectNames.NicifyVariableName(property.Name), typeof(ShowAttribute).Name);
                HelpBox_Layout( warning, MessageType.Warning, context: target as UnityEngine.Object );
            } else if( !Field_Layout( value, ObjectNames.NicifyVariableName( property.Name ) ) )
            {
                string warning = string.Format("{0} doesn't support {1} types", typeof(ShowAttribute).Name, property.PropertyType.Name);
                HelpBox_Layout( warning, MessageType.Warning, context: target as UnityEngine.Object );
            }
        }
        #endregion NativeProperty_Layout()

        #region NonSerializedField_Layout()
        public static void NonSerializedField_Layout( object target, FieldInfo field )
        {
            object value = field.GetValue( target );

            if( value == null )
            {
                string warning = string.Format("{0} is null. {1} doesn't support reference types with null value", ObjectNames.NicifyVariableName(field.Name), typeof(ShowAttribute).Name);
                HelpBox_Layout( warning, MessageType.Warning, context: target as UnityEngine.Object );
            } else if( !Field_Layout( value, ObjectNames.NicifyVariableName( field.Name ) ) )
            {
                string warning = string.Format("{0} doesn't support {1} types", typeof(ShowAttribute).Name, field.FieldType.Name);
                HelpBox_Layout( warning, MessageType.Warning, context: target as UnityEngine.Object );
            }
        }
        #endregion NonSerializedField_Layout()

        #region HorizontalLine()
        public static void HorizontalLine( Rect rect, float height, Color color )
        {
            rect.height = height;
            EditorGUI.DrawRect( rect, color );
        }
        #endregion HorizontalLine()

        #region HelpBox()
        public static void HelpBox( Rect rect, string message, MessageType type, UnityEngine.Object context = null, bool logToConsole = false )
        {
            EditorGUI.HelpBox( rect, message, type );

            if( logToConsole )
            {
                DebugLogMessage( message, type, context );
            }
        }
        #endregion HelpBox()

        #region HelpBox_Layout()
        public static void HelpBox_Layout( string message, MessageType type, UnityEngine.Object context = null, bool logToConsole = false )
        {
            EditorGUILayout.HelpBox( message, type );

            if( logToConsole )
            {
                DebugLogMessage( message, type, context );
            }
        }
        #endregion HelpBox_Layout()

        #region Field_Layout()
        public static bool Field_Layout( object value, string label )
        {
            using( new EditorGUI.DisabledScope( disabled: true ) )
            {
                bool isDrawn = true;
                Type valueType = value.GetType();

                if( valueType == typeof( bool ) )
                {
                    EditorGUILayout.Toggle( label, (bool)value );
                } else if( valueType == typeof( short ) )
                {
                    EditorGUILayout.IntField( label, (short)value );
                } else if( valueType == typeof( ushort ) )
                {
                    EditorGUILayout.IntField( label, (ushort)value );
                } else if( valueType == typeof( int ) )
                {
                    EditorGUILayout.IntField( label, (int)value );
                } else if( valueType == typeof( uint ) )
                {
                    EditorGUILayout.LongField( label, (uint)value );
                } else if( valueType == typeof( long ) )
                {
                    EditorGUILayout.LongField( label, (long)value );
                } else if( valueType == typeof( ulong ) )
                {
                    EditorGUILayout.TextField( label, ((ulong)value).ToString() );
                } else if( valueType == typeof( float ) )
                {
                    EditorGUILayout.FloatField( label, (float)value );
                } else if( valueType == typeof( double ) )
                {
                    EditorGUILayout.DoubleField( label, (double)value );
                } else if( valueType == typeof( string ) )
                {
                    EditorGUILayout.TextField( label, (string)value );
                } else if( valueType == typeof( Vector2 ) )
                {
                    EditorGUILayout.Vector2Field( label, (Vector2)value );
                } else if( valueType == typeof( Vector3 ) )
                {
                    EditorGUILayout.Vector3Field( label, (Vector3)value );
                } else if( valueType == typeof( Vector4 ) )
                {
                    EditorGUILayout.Vector4Field( label, (Vector4)value );
                } else if( valueType == typeof( Vector2Int ) )
                {
                    EditorGUILayout.Vector2IntField( label, (Vector2Int)value );
                } else if( valueType == typeof( Vector3Int ) )
                {
                    EditorGUILayout.Vector3IntField( label, (Vector3Int)value );
                } else if( valueType == typeof( Color ) )
                {
                    EditorGUILayout.ColorField( label, (Color)value );
                } else if( valueType == typeof( Bounds ) )
                {
                    EditorGUILayout.BoundsField( label, (Bounds)value );
                } else if( valueType == typeof( Rect ) )
                {
                    EditorGUILayout.RectField( label, (Rect)value );
                } else if( valueType == typeof( RectInt ) )
                {
                    EditorGUILayout.RectIntField( label, (RectInt)value );
                } else if( typeof( UnityEngine.Object ).IsAssignableFrom( valueType ) )
                {
                    EditorGUILayout.ObjectField( label, (UnityEngine.Object)value, valueType, true );
                } else if( valueType.BaseType == typeof( Enum ) )
                {
                    EditorGUILayout.EnumPopup( label, (Enum)value );
                } else if( valueType.BaseType == typeof( TypeInfo ) )
                {
                    EditorGUILayout.TextField( label, value.ToString() );
                } else
                {
                    isDrawn = false;
                }

                return isDrawn;
            }
        }
        #endregion Field_Layout()


        #region GetIndentLength()
        public static float GetIndentLength( Rect sourceRect )
        {
            Rect indentRect = EditorGUI.IndentedRect(sourceRect);
            float indentLength = indentRect.x - sourceRect.x;

            return indentLength;
        }
        #endregion GetIndentLength()

        #region GetFormattedText()
        public static string GetFormattedText( string text )
        {
            if( !text.StartsWith( "@" ) ) return text;

            // Parse formula in text
            return text;
        }
        #endregion GetFormattedText()


        #region DebugLogMessage()
        private static void DebugLogMessage( string message, MessageType type, UnityEngine.Object context )
        {
            switch( type )
            {
                case MessageType.None:
                case MessageType.Info:
                    Debug.Log( message, context );
                    break;
                case MessageType.Warning:
                    Debug.LogWarning( message, context );
                    break;
                case MessageType.Error:
                    Debug.LogError( message, context );
                    break;
            }
        }
        #endregion DebugLogMessage()
    }
}
