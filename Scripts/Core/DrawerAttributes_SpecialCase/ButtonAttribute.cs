using System;
using System.Runtime.CompilerServices;

namespace NiceAttributes
{
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
