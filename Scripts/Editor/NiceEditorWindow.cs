using System;
using UnityEditor;
using UnityEngine;

namespace NiceAttributes.Editor
{
    [Serializable]
    public abstract class NiceEditorWindow : EditorWindow
    {
        protected virtual bool DrawInspector { get; } = true;
        
        private ClassContext _rootClass;
        private Vector2 _scrollPosition;

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
            _rootClass = ClassContext.CreateContext( type, this, 0, additionalSkipTypes, true );
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

            if( _rootClass != null && DrawInspector )
            {
                _scrollPosition = EditorGUILayout.BeginScrollView( _scrollPosition );

                // Draw our custom Inspector
                _rootClass.Draw();
                
                EditorGUILayout.EndScrollView();
            }

            OnPostGUI();
        }
        #endregion OnGUI()
    }
}