using System;
using UnityEditor;

namespace NiceAttributes.Editor
{
    [Serializable]
    public abstract class NiceEditorWindow : EditorWindow
    {
        protected virtual bool DrawInspector { get; } = true;
        
        private ClassContext rootClass;

        protected virtual void OnPreGUI() {}
        protected virtual void OnPostGUI() {}

        #region OnEnable()
        protected virtual void OnEnable()
        {
            if( !DrawInspector ) return;
            
            var additionalSkipTypes = new Type[]
            {
                typeof(NiceEditorWindow),
                typeof(EditorWindow)
            };

            var type = this.GetType();
            rootClass = ClassContext.CreateContext( type, this, 0, additionalSkipTypes, true );
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

            if( rootClass != null && DrawInspector )
            {
                // Draw our custom Inspector
                rootClass.Draw();
            }

            OnPostGUI();
        }
        #endregion OnGUI()
    }
}