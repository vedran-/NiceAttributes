using UnityEngine;
using UnityEditor;
using System.Reflection;

namespace NiceAttributes.Editor
{
    [CustomPropertyDrawer( typeof( OnGUIAttribute ), true )]
    public class OnGUIPropertyDrawer : PropertyDrawerBase
    {
        #region RunGUIMethod()
        public static void RunGUIMethod( object obj, string methodName )
        {
            if( obj == null || methodName == null ) return;

            // Get the type of the parent object
            var parentObjectType = obj.GetType();

            // Get the MethodInfo for method with that name
            var onGUIMethod = parentObjectType.GetMethod( methodName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static );

            // Check if the method exists
            if( onGUIMethod != null )
            {
                // Invoke the method on the parent object
                onGUIMethod.Invoke( obj, null );
            } else
            {
                // Handle the case where the method does not exist
                EditorGUILayout.HelpBox( $"Could not find method '{methodName}' on object '{obj}'!", MessageType.Error );
                Debug.LogError( $"Method {methodName} not found on the parent object!", obj as Object );
            }
        }
        #endregion RunGUIMethod()

        #region MeasureGUIMethodSize()
        public static Rect MeasureGUIMethodSize( object obj, string methodName )
        {
            if( string.IsNullOrEmpty( methodName ) ) return Rect.zero;

            // Begin a GUILayout area that is effectively invisible.
            GUILayout.BeginArea( new Rect( 0, 0, 0, 0 ) );
            var rect = EditorGUILayout.BeginVertical();

            // Run method we want to measure the size for
            RunGUIMethod( obj, methodName );
#if false
            // Use GUILayout calls to layout your GUI elements.
            // For example, let's measure a button:
            GUIContent content = new GUIContent("My Button");
            GUIStyle style = GUI.skin.button;

            // Calculate the size required for the button with the current style.
            GUILayoutOption[] layoutOptions = {
                GUILayout.ExpandWidth(false),
                GUILayout.ExpandHeight(false)
            };
            Rect rect = GUILayoutUtility.GetRect( content, style, layoutOptions );

            // Now rect contains the size that the button would take up.
            // Do something with the size information here...

            GUILayout.EndVertical();
#endif
            EditorGUILayout.EndVertical();
            GUILayout.EndArea();

            // You can now use rect.width and rect.height to know the size needed for the button.
            return rect;
        }
        #endregion MeasureGUIMethodSize()

        float preHeight = 0;

        #region GetPropertyHeight_Internal()
        protected override float GetPropertyHeight_Internal( SerializedProperty property, GUIContent label )
        {
            var onGuiAttribute = attribute as OnGUIAttribute;
            var parentObject = PropertyUtility.GetTargetObjectWithProperty( property );
            //var parentObject = property.serializedObject.targetObject;

            var preH = MeasureGUIMethodSize( parentObject, onGuiAttribute.PreDrawMethodName ).height;
            if( Event.current.type == EventType.Repaint ) {
                preHeight = preH;
                //Debug.Log( $"Pre: {preHeight}, Post: {postHeight}, {Event.current.type}" );
            }

            return GetPropertyHeight( property ) + preHeight;
        }
        #endregion GetPropertyHeight_Internal()


        protected override void OnGUI_Internal( Rect rect, SerializedProperty property, GUIContent label )
        {
            EditorGUI.BeginProperty( rect, label, property );
            

            var onGuiAttribute = attribute as OnGUIAttribute;
            var parentObject = PropertyUtility.GetTargetObjectWithProperty( property );
            //var parentObject = property.serializedObject.targetObject;


#if false
            // Start the custom area using the position rect
            //GUI.BeginGroup( rect, EditorStyles.helpBox );
            //rect.x = rect.y = 0;
            //GUILayout.BeginArea( rect, EditorStyles.helpBox );
            GUILayout.BeginArea( rect, new GUIContent( "??" ) );

            // Now use GUILayout functions to draw your controls
            GUILayout.Label( "This is a label inside BeginArea", EditorStyles.helpBox );
            GUILayout.Label( "This is a label inside BeginArea", EditorStyles.helpBox );
            GUILayout.Label( "This is a label inside BeginArea", EditorStyles.helpBox );
            GUILayout.Label( "This is a label inside BeginArea", EditorStyles.helpBox );
            GUILayout.Label( "This is a label inside BeginArea", EditorStyles.helpBox );
            GUILayout.Label( "This is a label inside BeginArea", EditorStyles.helpBox );

            DrawingUtil.DrawCheckeredRect( new Rect( -3000, -3000, 6000, 6000 ), color: Color.red.WithAlpha(0.15f) );
            //DrawingUtil.FillRect( new Rect( -3000, -3000, 6000, 6000 ), Color.red.WithAlpha(0.5f) );
            //GUI.Label( rect, "This is a label inside BeginArea", EditorStyles.helpBox );

            Debug.Log( $"Last rect: {GUILayoutUtility.GetLastRect()}, rect: {rect}" );

            // End the custom area
            GUILayout.EndArea();
            //GUI.EndGroup();
            Debug.Log( $"Last rect2: {GUILayoutUtility.GetLastRect()}" );

rect.yMin += preHeight;
EditorGUI.PropertyField( rect, property, true );
EditorGUI.EndProperty(); return;
#endif

            // Draw Pre-GUI
#if false
            if( preHeight > 0 )
            {
                var preRect = rect;
                preRect.height = preHeight;

                //GUI.color = Color.white;
                //GUI.BeginGroup( preRect, EditorStyles.helpBox );
                //GUI.BeginClip( preRect );
                GUILayout.BeginArea( preRect );
                //FillRect( preRect, Color.red );

                GUILayout.Label( "HELLO" );
                GUILayout.Button( "Click Me" );

                RunGUIMethod( parentObject, onGuiAttribute.PreDrawMethodName );
                GUILayout.EndArea();
                //GUI.EndClip();
                //GUI.EndGroup();

                //Debug.Log( $"Pre-rect: {preRect}, {Event.current.type}" );
            }
#else
            RunGUIMethod( parentObject, onGuiAttribute.PreDrawMethodName );
#endif

            // Use original editor to draw property
            rect.yMin += preHeight;
            EditorGUI.PropertyField( rect, property, true );

            // Draw Post-GUI - no need for BeginArea() - just draw normally
            RunGUIMethod( parentObject, onGuiAttribute.PostDrawMethodName );

            EditorGUI.EndProperty();
        }
    }
}
