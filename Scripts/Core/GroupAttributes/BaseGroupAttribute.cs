using UnityEditor;

namespace NiceAttributes
{
    public abstract class BaseGroupAttribute : MetaAttribute
    {
        public const EColor ColorNotSet = (EColor)0x00000002;

        public string   GroupName { get; private set; }

        public string   Label { get; set; } = null;
        public bool     ShowLabel { get; set; } = false;
        public EColor   LabelColor { get; set; } = ColorNotSet;
        public EColor   LabelShadowColor { get; set; } = ColorNotSet;
        public EColor   LabelBackColor { get; set; } = ColorNotSet;
        public EColor   BackColor { get; set; } = ColorNotSet;

        public float    InsideLabelWidth { get; set; } = 0;
        public float    InsideFieldWidth { get; set; } = 0;

        protected BaseGroupAttribute( string groupName = "", int lineNumber = 0 )
            : base( lineNumber )
        {
            GroupName = groupName;
        }


#if UNITY_EDITOR
        /// <summary>
        /// GUI code to apply when this group starts.
        /// </summary>
        /// <returns>Returns true if group can be drawn, or false if not to draw the group.</returns>
        public abstract bool OnGUI_GroupStart();

        /// <summary>
        /// GUI code to apply when this group ends.
        /// </summary>
        public abstract void OnGUI_GroupEnd();


        #region [Util] SetLabelAndFieldWidth()
        float originalLabelWidth = 0, originalFieldWidth = 0;
        /// <summary>
        /// NOTE: It has to be called AFTER call to BeginHorizontal or BeginVertical, as those also change label and field width
        /// </summary>
        protected void SetLabelAndFieldWidth()
        {
            originalLabelWidth = EditorGUIUtility.labelWidth;
            originalFieldWidth = EditorGUIUtility.fieldWidth;

            // This has to be AFTER BeginHorizontal()
            if( InsideLabelWidth != 0 ) EditorGUIUtility.labelWidth = InsideLabelWidth;
            if( InsideFieldWidth != 0 ) EditorGUIUtility.fieldWidth = InsideFieldWidth;
        }
        #endregion SetLabelAndFieldWidth()

        #region [Util] RestoreLabelAndFieldWidth()
        protected void RestoreLabelAndFieldWidth()
        {
            // Reset to default to avoid affecting other Editor GUI elements
            if( InsideLabelWidth != 0 ) EditorGUIUtility.labelWidth = originalLabelWidth;
            if( InsideFieldWidth != 0 ) EditorGUIUtility.fieldWidth = originalFieldWidth;
        }
        #endregion RestoreLabelAndFieldWidth()
#endif
    }
}
