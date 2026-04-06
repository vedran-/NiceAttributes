using System;
using System.Collections.Generic;
using System.Linq;

namespace NiceAttributes.Editor.Grouping
{
    public static class GroupResolver
    {
        public static (Dictionary<string, ClassContext.GroupInfo> groups, List<ClassContext.ClassItem> members) BuildDisplayTree(IEnumerable<ClassContext.ClassItem> orderedMembers)
        {
            var groups = new Dictionary<string, ClassContext.GroupInfo>();

            var rootGroup = new ClassContext.GroupInfo("root");
            groups["root"] = rootGroup;
            rootGroup.groups = new ClassContext.GroupInfo[] { rootGroup };

            ClassContext.GroupInfo GetGroup(string groupName)
            {
                if (groups.TryGetValue(groupName, out var groupInfo)) return groupInfo;

                var gs = groupName.Split('/', StringSplitOptions.None);
                var sb = new System.Text.StringBuilder();
                var allGroups = new List<ClassContext.GroupInfo>();
                foreach (var name in gs)
                {
                    if (sb.Length > 0) sb.Append('/');
                    sb.Append(name);
                    var gname = sb.ToString();

                    if (!groups.TryGetValue(gname, out var gi))
                    {
                        var n = new ClassContext.GroupInfo(gname);
                        groups[gname] = n;
                        allGroups.Add(n);
                        n.groups = allGroups.ToArray();
                    }
                    else allGroups.Add(gi);
                }

                return allGroups.Last();
            }

            var items = new List<ClassContext.ClassItem>();
            foreach (var member in orderedMembers)
            {
                var errors = new List<string>();
                int deepestLevel = -1;
                ClassContext.GroupInfo mainGroupInfo = null;
                var groupAttributes = member.memberInfo.GetCustomAttributes<NiceAttributes.Model.BaseGroupAttribute>();
                foreach (var groupAttribute in groupAttributes)
                {
                    var groupName = groupAttribute != null ? $"root/{groupAttribute.GroupName}" : "root";
                    var groupInfo = GetGroup(groupName);

                    if (groupInfo.groupAttribute == null || groupInfo.groupAttribute is NiceAttributes.Model.GroupAttribute) groupInfo.groupAttribute = groupAttribute;

                    if (groupInfo.groups != null && groupInfo.groups.Length > deepestLevel)
                    {
                        deepestLevel = groupInfo.groups.Length;
                        mainGroupInfo = groupInfo;
                    }

                    if (groupAttribute != null && groupAttribute is not NiceAttributes.Model.GroupAttribute
                        && groupInfo.groupAttribute.GetType() != groupAttribute.GetType())
                    {
                        errors.Add($"Group type {groupAttribute.GetType().Name} is different from original {groupInfo.groupAttribute.GetType().Name} for group '{groupInfo.groupName}'!");
                    }
                }
                if (mainGroupInfo == null)
                {
                    mainGroupInfo = rootGroup;
                }

                member.group = mainGroupInfo;
                member.errorMessage = errors.Count > 0 ? string.Join("\n", errors) : null;

                int mostGroupsIdx = -1, mostGroupsCount = -1;
                for (int idx = 0; idx < items.Count; idx++)
                {
                    int len = Mathf.Min(items[idx].group.groups.Length, mainGroupInfo.groups.Length);
                    for (int j = 0; j < len; j++)
                    {
                        if (items[idx].group.groups[j] != mainGroupInfo.groups[j]) break;

                        if (j >= mostGroupsCount)
                        {
                            mostGroupsIdx = idx;
                            mostGroupsCount = j;
                        }
                    }
                }

                if (mostGroupsIdx < 0) items.Add(member);
                else items.Insert(mostGroupsIdx + 1, member);
            }

            return (groups, items);
        }
    }
}
