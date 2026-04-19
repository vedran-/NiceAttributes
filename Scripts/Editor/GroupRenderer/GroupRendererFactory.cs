using UnityEditor;
using System.Collections.Generic;

namespace NiceAttributes.Editor.GroupRenderer
{
    public static class GroupRendererFactory
    {
        private static readonly Stack<BaseGroupRenderer> _rendererStack = new();

        public static bool Start(BaseGroupAttribute attr, object target)
        {
            BaseGroupRenderer renderer = attr switch
            {
                BoxGroupAttribute => new BoxGroupRenderer(),
                FoldoutAttribute => new FoldoutGroupRenderer(),
                HorizontalGroupAttribute => new HorizontalGroupRenderer(),
                VerticalGroupAttribute => new VerticalGroupRenderer(),
                TabGroupAttribute => new TabGroupRenderer(),
                GroupAttribute => new GroupGroupRenderer(),
                _ => throw new System.NotImplementedException($"No renderer for {attr.GetType().Name}")
            };
            _rendererStack.Push(renderer);
            return renderer.Start(attr, target);
        }

        public static void End(BaseGroupAttribute attr, object target)
        {
            if (_rendererStack.Count == 0) return;

            BaseGroupRenderer renderer = _rendererStack.Pop();
            renderer?.End(attr, target);
        }
    }
}