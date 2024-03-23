using UnityEditor;

namespace NiceAttributes.Editor
{
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
            // If the class we need to display doesn't use NiceAttributes, then just use the default Inspector
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
