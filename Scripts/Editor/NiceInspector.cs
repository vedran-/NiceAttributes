using System;
using NiceAttributes.Editor.PropertyDrawers_SpecialCase;
using UnityEditor;
using UnityEngine;

namespace NiceAttributes.Editor
{
#if !DONT_USE_NICE_INSPECTOR
    [CanEditMultipleObjects]
    [CustomEditor(typeof(UnityEngine.Object), true)]
#endif
    public class NiceInspector : UnityEditor.Editor
    {
        private ClassContext _rootClass;

        protected virtual void OnEnable()
        {
            if (target == null)
            {
                return;
            }

            try
            {
                _rootClass = ClassContext.CreateContext(target.GetType(), serializedObject.targetObject, 0);
                if (_rootClass.HasNiceAttributes)
                {
                    ClassContext.ConnectWithSerializedProperties(_rootClass, serializedObject.GetIterator());
                }
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
        }

        protected virtual void OnDisable()
        {
            ReorderableListPropertyDrawer.Instance.ClearCache();
        }


        public override void OnInspectorGUI()
        {
            // If the class we need to display doesn't use NiceAttributes, then just use the default Inspector
            if( _rootClass == null || !_rootClass.HasNiceAttributes )
            {
                DrawDefaultInspector();
                return;
            }

            serializedObject.Update();

            // Draw our custom Inspector
            _rootClass.Draw();

            serializedObject.ApplyModifiedProperties();
        }
    }
}
