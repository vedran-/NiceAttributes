using System;
using System.Runtime.CompilerServices;

namespace NiceAttributes
{
    public enum ButtonEnableMode
    {
        /// <summary>
        /// Button should always be active
        /// </summary>
        Always,
        /// <summary>
        /// Button should be active only in editor
        /// </summary>
        Editor,
        /// <summary>
        /// Button should be active only in playmode
        /// </summary>
        Playmode
    }

    [AttributeUsage( AttributeTargets.Method, AllowMultiple = false, Inherited = true )]
    public class ButtonAttribute : SpecialCaseDrawerAttribute
    {
        public string Text { get; private set; }
        public ButtonEnableMode SelectedEnableMode { get; private set; }

        public ButtonAttribute( string text = null, ButtonEnableMode enabledMode = ButtonEnableMode.Always, [CallerLineNumber] int lineNumber = 0 )
            : base( lineNumber )
        {
            this.Text = text;
            this.SelectedEnableMode = enabledMode;
        }
    }
}
