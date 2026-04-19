using UnityEditor;

namespace NiceAttributes.Editor.GroupRenderer
{
    public abstract class BaseGroupRenderer
    {
        protected virtual bool OnGUI_GroupStart(BaseGroupAttribute attr, object target) => true;
        protected virtual void OnGUI_GroupEnd(BaseGroupAttribute attr, object target) { }

        private float _originalLabelWidth;
        private float _originalFieldWidth;

        public bool Start(BaseGroupAttribute attr, object target)
        {
            _originalLabelWidth = EditorGUIUtility.labelWidth;
            _originalFieldWidth = EditorGUIUtility.fieldWidth;

            if (attr.InsideLabelWidth != 0) EditorGUIUtility.labelWidth = attr.InsideLabelWidth;
            if (attr.InsideFieldWidth != 0) EditorGUIUtility.fieldWidth = attr.InsideFieldWidth;

            return OnGUI_GroupStart(attr, target);
        }

        public void End(BaseGroupAttribute attr, object target)
        {
            OnGUI_GroupEnd(attr, target);

            if (attr.InsideLabelWidth != 0) EditorGUIUtility.labelWidth = _originalLabelWidth;
            if (attr.InsideFieldWidth != 0) EditorGUIUtility.fieldWidth = _originalFieldWidth;
        }
    }
}