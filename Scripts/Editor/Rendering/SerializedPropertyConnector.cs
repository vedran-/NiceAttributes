using System.Linq;
using System.Reflection;
using NiceAttributes.Editor;
using NiceAttributes.Editor.Utility;
using UnityEditor;
using UnityEngine;

namespace NiceAttributes.Editor.Rendering
{
    public static class SerializedPropertyConnector
    {
        public static void Connect(ClassContext ctx, SerializedProperty property)
        {
            var parentPath = property.propertyPath;
            if (!property.NextVisible(true)) return;

            do
            {
                if (!property.propertyPath.StartsWith(parentPath)) break;

                var item = ctx._displayedMembers.FirstOrDefault(d =>
                    d.memberInfo != null &&
                    (
                        d.memberInfo.Name == property.name ||
                        (d.memberInfo is System.Reflection.PropertyInfo propInfo && $"<{propInfo.Name}>k__BackingField" == property.name)
                    )
                );
                if (item == null)
                {
                    if (property.name == "m_Script")
                    {
                        var mScript = new ClassContext.ClassItem() { serializedProperty = property.Copy() };
                        ctx._displayedMembers.Insert(0, mScript);
                    }
                    else
                    {
                        var isHidden = PropertyUtility.GetAttribute<HideAttribute>(property) != null
                            || PropertyUtility.GetPropertyType(property).GetCustomAttribute<HideAttribute>() != null;

                        if (!isHidden)
                        {
                            Debug.LogWarning($"[NiceAttributes] Could not find ClassItem for serialized property '{property.name}' — this may indicate a missing [Show] attribute or a property from another editor. Target: {ctx._targetObject?.GetType().Name}");
                        }
                    }
                    continue;
                }

                item.serializedProperty = property.Copy();

                if (property.hasChildren && property.isExpanded)
                {
                    if (item.classContext != null)
                    {
                        Connect(item.classContext, property.Copy());
                    }
                    else
                    {
                        Debug.LogWarning($"SerializedProperty {property.name} has children, but '{item.memberInfo.Name}' has no class context!");
                    }
                }

            } while (property.NextVisible(false));
        }
    }
}
