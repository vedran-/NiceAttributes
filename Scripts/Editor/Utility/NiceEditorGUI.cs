using System;
using System.Collections;
using System.Collections.Generic;
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

        private static readonly GUIStyle _buttonStyle = new GUIStyle(GUI.skin.button) { richText = true };

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
        private static void PropertyField_Implementation( Rect rect, SerializedProperty property, 
            bool includeChildren, PropertyFieldFunction propertyFieldFunction )
        {
            var specialCaseAttribute = PropertyUtility.GetAttribute<SpecialCaseDrawerAttribute>(property);
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
                var buttonAttribute = (ButtonAttribute)methodInfo.GetCustomAttributes(typeof(ButtonAttribute), true)[0];
                var buttonText = string.IsNullOrEmpty(buttonAttribute.Text) ? ObjectNames.NicifyVariableName(methodInfo.Name) : buttonAttribute.Text;
                var buttonEnabled = ButtonUtility.IsEnabled( target, methodInfo );

                var mode = buttonAttribute.SelectedEnableMode;
                buttonEnabled &=
                    mode == ButtonEnableMode.Always ||
                    mode == ButtonEnableMode.Editor && !Application.isPlaying ||
                    mode == ButtonEnableMode.Playmode && Application.isPlaying;

                var methodIsCoroutine = methodInfo.ReturnType == typeof(IEnumerator);
                if( methodIsCoroutine )
                {
                    buttonEnabled &= Application.isPlaying;
                }

                EditorGUI.BeginDisabledGroup( !buttonEnabled );

                if( GUILayout.Button( buttonText, _buttonStyle ) )
                {
                    var defaultParams = methodInfo.GetParameters().Select(p => p.DefaultValue).ToArray();
                    var methodResult = methodInfo.Invoke(target, defaultParams) as IEnumerator;

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
                string warning = nameof(ButtonAttribute) + $" works only on methods with no parameters [{methodInfo.Name}]";
                HelpBox_Layout( warning, MessageType.Warning, context: target as UnityEngine.Object, logToConsole: true );
            }
        }
        #endregion Button()

        #region NativeProperty_Layout()
        internal static void NativeProperty_Layout( object target, PropertyInfo property )
        {
            var value = property.GetValue(target, null);
            var label = new GUIContent( ObjectNames.NicifyVariableName(property.Name) );
            if( !Field_Layout( value, property.PropertyType, label, !property.CanWrite, out var outValue ) )
            {
                var warning = $"{nameof(ShowAttribute)} doesn't support {property.PropertyType.Name} types";
                HelpBox_Layout( warning, MessageType.Warning, context: target as UnityEngine.Object );
                return;
            }

            if( property.CanWrite ) {
                property.SetValue(target, outValue, null);
            }
        }
        #endregion NativeProperty_Layout()

        #region NonSerializedField_Layout()
        internal static void NonSerializedField_Layout( object target, FieldInfo field )
        {
            var value = field.GetValue( target );
            var label = new GUIContent( ObjectNames.NicifyVariableName(field.Name) );
            if( !Field_Layout( value, field.FieldType, label, false, out var outValue ) )
            {
                var warning = $"{nameof(ShowAttribute)} doesn't support {field.FieldType.Name} types";
                HelpBox_Layout( warning, MessageType.Warning, context: target as UnityEngine.Object );
                return;
            }

            field.SetValue(target, outValue);
        }
        #endregion NonSerializedField_Layout()

        #region HorizontalLine()
        public static void HorizontalLine( Rect rect, float height, Color color )
        {
            rect.height = height;
            EditorGUI.DrawRect( rect, color );
        }
        #endregion HorizontalLine()


        #region [HelpBox Util] CreateHelpBoxStyles()
        static GUIStyle helpBoxTextStyle = null, helpBoxStyle = null;
        static void CreateHelpBoxStyles( TextAnchor textAlignment = TextAnchor.MiddleLeft )
        {
            // Lazily create GUI styles
            if( helpBoxTextStyle == null ) helpBoxTextStyle = new GUIStyle( GUI.skin.label ) {
                richText = true,
                wordWrap = true,
                alignment = textAlignment,
                padding = new RectOffset( 4, 4, 10, 10 ) // Add padding inside the text area
            };
            // Create a style for the box with a border
            if( helpBoxStyle == null ) helpBoxStyle = new GUIStyle( GUI.skin.window ) {
                padding = new RectOffset( 1, 1, 1, 1 ), // Padding for the border
                margin = new RectOffset( 4, 4, 4, 4 )   // Margin outside the box
            };
        }
        #endregion CreateHelpBoxStyles()

        #region HelpBox()
        public static void HelpBox( Rect rect, string message, MessageType messageType,
            UnityEngine.Object context = null, bool logToConsole = false )
        {
#if true
            CreateHelpBoxStyles();

            // Assuming 'Rect rect' is defined and passed to this method
            GUI.BeginGroup( rect, helpBoxStyle ); // Begin a group within the specified rectangle

            // Calculate rectangles for the icon and text based on the main rect's dimensions
            Rect iconRect = new Rect( 10, rect.height/2 - 18, 36, 36 ); // Adjust padding as needed

            // Display the icon based on the messageType
            GUIContent iconContent = null;
            switch( messageType ) {
                case MessageType.Info: iconContent = EditorGUIUtility.IconContent( "console.infoicon" ); break;
                case MessageType.Warning: iconContent = EditorGUIUtility.IconContent( "console.warnicon" ); break;
                case MessageType.Error: iconContent = EditorGUIUtility.IconContent( "console.erroricon" ); break;
            }
            if( iconContent != null ) {
                GUI.Label( iconRect, iconContent ); // Draw the icon
            }


            Rect textRect = new Rect( 46, 0, rect.width-46, rect.height ); // Adjust size based on the icon's position and rect's width


            // Draw the text next to the icon
            GUI.Label( textRect, new GUIContent( message ), helpBoxTextStyle );
            GUI.EndGroup(); // End the group
#else
            EditorGUI.HelpBox( rect, message, messageType );
#endif
            //EditorGUI.HelpBox( rect, message, messageType );

            if( logToConsole )
            {
                DebugLogMessage( message, messageType, context );
            }
        }
        #endregion HelpBox()

        #region HelpBox_Layout()
        public static void HelpBox_Layout( string message, MessageType messageType, 
            UnityEngine.Object context = null, bool logToConsole = false )
        {
            //EditorGUILayout.HelpBox( message, messageType );

            CreateHelpBoxStyles();

            EditorGUILayout.BeginVertical( helpBoxStyle ); // Begin a vertical group with the box style to include a border
            EditorGUILayout.BeginHorizontal(); // Begin a horizontal group to align icon and text

            // Display the icon based on the messageType
            GUIContent iconContent = null;
            switch( messageType )
            {
                case MessageType.Info: iconContent = EditorGUIUtility.IconContent( "console.infoicon" ); break;
                case MessageType.Warning: iconContent = EditorGUIUtility.IconContent( "console.warnicon" ); break;
                case MessageType.Error: iconContent = EditorGUIUtility.IconContent( "console.erroricon" ); break;
            }
            if( iconContent != null ) { // Set a fixed size for the icon
                EditorGUILayout.LabelField( iconContent, GUILayout.Width( 36 ), GUILayout.ExpandHeight( true ) );
            }

            // Ensure the text field expands to fit the available width
            EditorGUILayout.LabelField( new GUIContent( message ), helpBoxTextStyle, GUILayout.ExpandWidth( true ) );

            EditorGUILayout.EndHorizontal(); // End the horizontal group
            EditorGUILayout.EndVertical(); // End the vertical group (with the border)#else

            if( logToConsole ) {
                DebugLogMessage( message, messageType, context );
            }
        }
        #endregion HelpBox_Layout()


        #region Field_Layout()
        private static bool Field_Layout( object value, Type valueType, GUIContent label, bool readOnly, out object outValue )
        {
            using( new EditorGUI.DisabledScope( disabled: readOnly ) )
            {
                var isDrawn = true;
                
                if( typeof( UnityEngine.Object ).IsAssignableFrom( valueType ) )
                {
                    outValue = EditorGUILayout.ObjectField( label, (UnityEngine.Object)value, valueType, true );
                } else if( valueType == typeof( int ) )
                {
                    outValue = EditorGUILayout.IntField( label, (int)value );
                } else if( valueType == typeof( bool ) )
                {
                    outValue = EditorGUILayout.Toggle( label, (bool)value );
                } else if( valueType == typeof( string ) )
                {
                    outValue = EditorGUILayout.TextField( label, (string)value );
                } else if( valueType == typeof( float ) )
                {
                    outValue = EditorGUILayout.FloatField( label, (float)value );
                } else if( valueType == typeof( double ) )
                {
                    outValue = EditorGUILayout.DoubleField( label, (double)value );
                } else if( valueType == typeof( short ) )
                {
                    outValue = EditorGUILayout.IntField( label, (short)value );
                } else if( valueType == typeof( ushort ) )
                {
                    outValue = EditorGUILayout.IntField( label, (ushort)value );
                } else if( valueType == typeof( uint ) )
                {
                    outValue = EditorGUILayout.LongField( label, (uint)value );
                } else if( valueType == typeof( long ) )
                {
                    outValue = EditorGUILayout.LongField( label, (long)value );
                } else if( valueType == typeof( ulong ) )
                {
                    var val = EditorGUILayout.TextField( label, ((ulong)value).ToString() );
                    if( ulong.TryParse(val, out var outVal) )
                    {
                        outValue = outVal;
                    } else
                    {
                        //HelpBox_Layout( $"Invalid value '{val}'", MessageType.Warning );
                        outValue = value;   // Don't change input value
                    }
                } else if( valueType == typeof( Vector2 ) )
                {
                    outValue = EditorGUILayout.Vector2Field( label, (Vector2)value );
                } else if( valueType == typeof( Vector3 ) )
                {
                    outValue = EditorGUILayout.Vector3Field( label, (Vector3)value );
                } else if( valueType == typeof( Vector4 ) )
                {
                    outValue = EditorGUILayout.Vector4Field( label, (Vector4)value );
                } else if( valueType == typeof( Vector2Int ) )
                {
                    outValue = EditorGUILayout.Vector2IntField( label, (Vector2Int)value );
                } else if( valueType == typeof( Vector3Int ) )
                {
                    outValue = EditorGUILayout.Vector3IntField( label, (Vector3Int)value );
                } else if( valueType == typeof( Color ) )
                {
                    outValue = EditorGUILayout.ColorField( label, (Color)value );
                } else if( valueType == typeof( Bounds ) )
                {
                    outValue = EditorGUILayout.BoundsField( label, (Bounds)value );
                } else if( valueType == typeof( Rect ) )
                {
                    outValue = EditorGUILayout.RectField( label, (Rect)value );
                } else if( valueType == typeof( RectInt ) )
                {
                    outValue = EditorGUILayout.RectIntField( label, (RectInt)value );
                } else if( valueType.BaseType == typeof( Enum ) )
                {
                    outValue = EditorGUILayout.EnumPopup( label, (Enum)value );
                } else if( valueType.BaseType == typeof( TypeInfo ) )
                {
                    outValue = EditorGUILayout.TextField( label, value.ToString() );
                } else if( typeof(ICollection).IsAssignableFrom(valueType) )
                {
                    (isDrawn, outValue) = HandleCollection( value, label );
                } else
                {
                    isDrawn = false;
                    outValue = null;
                }

                return isDrawn;
            }
        }
        #endregion Field_Layout()

        private static bool foldedOut = true;
        private static (bool isDrawn, object outValue) HandleCollection( object value, GUIContent label )
        {
            var valueType = value.GetType();
            var array = valueType.IsArray ? (Array)value : null;
            var genericCollection = valueType.IsGenericType ? value as ICollection : null;

            var length = array != null ? array.Length 
                : genericCollection != null ? genericCollection.Count : -1;

            if( length < 0 )
            {
                HelpBox_Layout($"Invalid ICollection type '{valueType.Name}'", MessageType.Error);
                return (false, value);
            }

            var folded = EditorGUILayout.Foldout( foldedOut, label, true );
            if( folded != foldedOut ) { // Value changed
                foldedOut = folded;
                //EditorPrefs.SetBool( id, folded );
            }
            if( !foldedOut ) return (true, value);
            
            for( int i = 0; i < length; i++ )
            {
                var item = array != null ? array.GetValue(i) : genericCollection.Cast<object>().ElementAt(i);

                if( Field_Layout( item, item.GetType(), new GUIContent($"Element {i}"), false, out var outValue ) )
                {
                    // Set new value
                    if( outValue != value )
                    {
                        if( array != null ) array.SetValue( outValue, i );
                        else if( genericCollection != null )
                        {
                            if( genericCollection is IList list )
                            {
                                list[i] = outValue;
                            } else {
                                var tempCollection = Activator.CreateInstance(valueType, true);
                                var addMethod = valueType.GetMethod("Add");
                                var count = 0;
                                foreach( var element in genericCollection )
                                {
                                    addMethod.Invoke( tempCollection, new[] { count == i ? outValue : element } ); 
                                    count++;
                                }
                                genericCollection = tempCollection as ICollection;
                            }
                        }
                    }
                }
            }
            
            return (true, value);
        }

        
        #region GetIndentLength()
        public static float GetIndentLength( Rect sourceRect )
        {
            Rect indentRect = EditorGUI.IndentedRect(sourceRect);
            float indentLength = indentRect.x - sourceRect.x;

            return indentLength;
        }
        #endregion GetIndentLength()


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
