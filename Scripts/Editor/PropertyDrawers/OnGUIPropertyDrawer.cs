using System.Reflection;
using NiceAttributes.Editor.Utility;
using UnityEditor;
using UnityEngine;

namespace NiceAttributes.Editor.PropertyDrawers
{
    [CustomPropertyDrawer( typeof( OnGUIAttribute ), true )]
    public class OnGUIPropertyDrawer : PropertyDrawerBase
    {
        private float preHeight = 0, postHeight = 0;
        private Rect screenRect;

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

        private static Rect MeasureGUIMethodSize( object obj, string methodName )
        {
            if( string.IsNullOrEmpty( methodName ) ) return Rect.zero;

            // Begin a GUILayout area that is effectively invisible.
            GUILayout.BeginArea( new Rect( 0, 0, 0, 0 ) );
            var rect = EditorGUILayout.BeginVertical();
            RunGUIMethod( obj, methodName );    // Run method we want to measure the size for
            EditorGUILayout.EndVertical();
            GUILayout.EndArea();

            // You can now use rect.width and rect.height to know the size needed for the button.
            return rect;
        }

        protected override float GetPropertyHeight_Internal(SerializedProperty property, GUIContent label)
        {
            var onGuiAttribute = attribute as OnGUIAttribute;
            var parentObject = PropertyUtility.GetTargetObjectWithProperty(property);
            //var parentObject = property.serializedObject.targetObject;

            float preH = 0, postH = 0;
            if (!string.IsNullOrEmpty(onGuiAttribute.PreDrawMethodName))
            {
                preH = MeasureGUIMethodSize(parentObject, onGuiAttribute.PreDrawMethodName).height;
            }

            if (!string.IsNullOrEmpty(onGuiAttribute.PostDrawMethodName))
            {
                postH = MeasureGUIMethodSize(parentObject, onGuiAttribute.PostDrawMethodName).height;
            }

            //var preH = MeasureGUIMethodSize( parentObject, onGuiAttribute.PreDrawMethodName ).height;
            if (Event.current.type == EventType.Repaint)
            {
                preHeight = preH;
                postHeight = postH;
                //Debug.Log( $"Pre: {preHeight}, Post: {postHeight}, {Event.current.type}" );
            }

            return GetPropertyHeight(property) + preHeight + postHeight;
        }


        protected override void OnGUI_Internal(Rect rect, SerializedProperty property, GUIContent label)
        {
            if (Event.current.type == EventType.Repaint)
            {
                screenRect = GUILayoutUtility.GetLastRect();
            }

            EditorGUI.BeginProperty(rect, label, property);
            var onGuiAttribute = attribute as OnGUIAttribute;
            var parentObject = PropertyUtility.GetTargetObjectWithProperty(property);
            //var parentObject = property.serializedObject.targetObject;

            if (!string.IsNullOrEmpty(onGuiAttribute.PreDrawMethodName))
            {
                var preRect = screenRect;
                preRect.height = preHeight;
                GUILayout.BeginArea(preRect);
                RunGUIMethod(parentObject, onGuiAttribute.PreDrawMethodName);
                GUILayout.EndArea();
            }

            // Use original editor to draw property
            rect.yMin += preHeight;
            rect.yMax -= postHeight;
            EditorGUI.PropertyField(rect, property, true);

            // Draw Post-GUI - no need for BeginArea() - just draw normally
            if (!string.IsNullOrEmpty(onGuiAttribute.PostDrawMethodName))
            {
                var postRect = screenRect;
                postRect.yMin = postRect.yMax - postHeight;
                GUILayout.BeginArea(postRect);
                RunGUIMethod(parentObject, onGuiAttribute.PostDrawMethodName);
                GUILayout.EndArea();
            }

            EditorGUI.EndProperty();
        }
    }
}
