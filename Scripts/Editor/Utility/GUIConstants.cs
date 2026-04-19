namespace NiceAttributes.Editor.Utility
{
    /// <summary>
    /// Centralized constants for GUI dimensions, spacing, and layout values.
    /// Replaces magic numbers scattered across renderer and drawer files.
    /// </summary>
    public static class GUIConstants
    {
        // === Indentation & Spacing ===
        public const float ListElementTopIndent = 1.0f;
        public const float ListElementLeftIndent = 10.0f;
        public const float ListElementExtraHeight = 4.0f;

        // === HelpBox Layout ===
        public const int HelpBoxTextPadding = 4;
        public const int HelpBoxTextPaddingTopBottom = 10;
        public const int HelpBoxStylePadding = 1;
        public const int HelpBoxStyleMargin = 4;
        public const int HelpBoxIconMarginLeft = 10;
        public const int HelpBoxIconSize = 36;
        public const int HelpBoxIconVerticalOffset = 18;
        public const int HelpBoxTextOffset = 46;

        // === MinMaxSlider Layout ===
        public const float SliderPadding = 5.0f;

        // === ProgressBar Layout ===
        public const int ProgressBarLabelVerticalOffset = 2;

        // === InfoBox Layout ===
        public const float InfoBoxHeightMultiplier = 2.0f;
        public const float InfoBoxExtraHeight = 10.0f;
    }
}