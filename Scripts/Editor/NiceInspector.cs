using System;
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
        ClassContext    rootClass;

        #region OnEnable()
        protected virtual void OnEnable()
        {
            try
            {
                rootClass = ClassContext.CreateContext(target.GetType(), serializedObject.targetObject, 0);
                if (rootClass.HasNiceAttributes)
                {
                    ClassContext.ConnectWithSerializedProperties(rootClass, serializedObject.GetIterator());
                }
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
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
            // If the class we need to display doesn't use NiceAttributes, then just use the default Inspector
            if( rootClass == null || !rootClass.HasNiceAttributes )
            {
                DrawDefaultInspector();
                return;
            }

            serializedObject.Update();

            // Draw our custom Inspector
            rootClass.Draw();

            serializedObject.ApplyModifiedProperties();
        }
        #endregion OnInspectorGUI()
    }
}
