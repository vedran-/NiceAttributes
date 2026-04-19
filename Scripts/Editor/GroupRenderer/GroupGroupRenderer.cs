using UnityEditor;

namespace NiceAttributes.Editor.GroupRenderer
{
    /// <summary>
    /// Renderer for the generic [Group] placeholder attribute.
    /// Passes through without rendering anything — the actual group type
    /// is defined by a concrete group attribute on a different field in the same group.
    /// </summary>
    public class GroupGroupRenderer : BaseGroupRenderer
    {
    }
}