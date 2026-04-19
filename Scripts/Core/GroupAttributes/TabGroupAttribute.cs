using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;

namespace NiceAttributes
{
    [AttributeUsage( AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Method, AllowMultiple = true )]
    [Conditional("UNITY_EDITOR")]
    public class TabGroupAttribute : BaseGroupAttribute
    {
        public TabGroupAttribute(string groupName = "", [CallerLineNumber] int lineNumber = 0)
            : base(groupName, lineNumber) {}

        public class TabParent
        {
            public List<TabGroupAttribute>  tabGroups = null;
            public string[]                 tabHeader = null;
            public int                      selectedTabIdx = 0;
        }

        public TabParent tabParent = null;
        public bool IsSelectedTab => GetSelectedTab() == this;

        private int GetSelectedTabIdx()
        {
            if (tabParent == null || tabParent.tabGroups == null || tabParent.tabGroups.Count == 0) return -1;
            if (tabParent.selectedTabIdx >= tabParent.tabGroups.Count) tabParent.selectedTabIdx = tabParent.tabGroups.Count - 1;
            if (tabParent.selectedTabIdx < 0) tabParent.selectedTabIdx = 0;
            return tabParent.selectedTabIdx;
        }

        private TabGroupAttribute GetSelectedTab()
        {
            var idx = GetSelectedTabIdx();
            return idx >= 0 ? tabParent.tabGroups[idx] : null;
        }
    }
}