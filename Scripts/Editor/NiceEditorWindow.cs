using System;
using UnityEditor;
using UnityEngine;

namespace NiceAttributes.Editor
{
    [Serializable]
    public abstract class NiceEditorWindow : EditorWindow
    {
        private ClassContext rootClass;

        protected virtual void OnPreGUI() {}
        protected virtual void OnPostGUI() {}

        #region OnEnable()
        protected virtual void OnEnable()
        {
            var additionalSkipTypes = new Type[]
            {
                typeof(NiceEditorWindow),
                typeof(EditorWindow)
            };

            var type = this.GetType();
            rootClass = ClassContext.CreateContext(type, this, 0, additionalSkipTypes);
        }
        #endregion OnEnable()

        #region OnDisable()
        protected virtual void OnDisable()
        {
            ReorderableListPropertyDrawer.Instance.ClearCache();
        }
        #endregion OnDisable()


        #region OnGUI()
        private void OnGUI()
        {
            OnPreGUI();

            // If the class we need to display doesn't use NiceAttributes, then just use the default Inspector
            if( rootClass == null || !rootClass.HasNiceAttributes )
            {
                //DrawDefaultInspector();
                return;
            }

            // Draw our custom Inspector
            rootClass.Draw();

            OnPostGUI();
        }
        #endregion OnGUI()
    }
}