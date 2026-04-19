using NiceAttributes.Model;

namespace NiceAttributes
{
    public abstract class BaseGroupAttribute : MetaAttribute
    {
        public string   GroupName { get; private set; }
        public string   Title { get; set; } = null;
        public bool     ShowTitle { get; set; } = false;
        public NiceColor   TitleColor { get; set; } = NiceColor.ColorNotSet;
        public NiceColor   TitleShadowColor { get; set; } = NiceColor.ColorNotSet;
        public NiceColor   TitleBackColor { get; set; } = NiceColor.ColorNotSet;
        public NiceColor   GroupBackColor { get; set; } = NiceColor.ColorNotSet;
        public float    InsideLabelWidth { get; set; } = 0;
        public float    InsideFieldWidth { get; set; } = 0;

        protected BaseGroupAttribute( string groupName = "", int lineNumber = 0 )
            : base( lineNumber )
        {
            GroupName = groupName;
        }
    }
}