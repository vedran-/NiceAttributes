namespace NiceAttributes.Editor
{
    public partial class ClassContext
    {
        private class GroupInfo
        {
            public readonly string      groupName;
            public BaseGroupAttribute   groupAttribute = null;
            public GroupInfo[]          groups; // List of all groups this group belongs to - including itself
            internal TabGroupAttribute.TabParent tabParent;

            public GroupInfo(string name)
            {
                groupName = name;
            }

            internal GroupInfo GetParentGroup()
            {
                if (groups.Length < 2) return null;
                return groups[^2];
            }
        }
    }
}