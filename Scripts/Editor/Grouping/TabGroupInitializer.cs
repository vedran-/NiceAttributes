using System.Collections.Generic;
using System.Linq;
using NiceAttributes.GroupAttributes;

namespace NiceAttributes.Editor.Grouping
{
    public static class TabGroupInitializer
    {
        public static void Initialize(Dictionary<string, ClassContext.GroupInfo> groups)
        {
            foreach (var group in groups.Values)
            {
                if (group.groupAttribute is not TabGroupAttribute tabAtt) continue;

                var parentGroup = group.GetParentGroup();
                if (parentGroup == null)
                {
                    UnityEngine.Debug.LogError($"TabGroup '{group.groupName}' has no parent!");
                    continue;
                }

                if (parentGroup.tabParent == null)
                {
                    parentGroup.tabParent = new TabGroupAttribute.TabParent()
                    {
                        tabGroups = new List<TabGroupAttribute>() { tabAtt }
                    };
                }
                else
                {
                    if (!parentGroup.tabParent.tabGroups.Contains(tabAtt))
                        parentGroup.tabParent.tabGroups.Add(tabAtt);
                }

                tabAtt.tabParent = parentGroup.tabParent;
            }
        }
    }
}
