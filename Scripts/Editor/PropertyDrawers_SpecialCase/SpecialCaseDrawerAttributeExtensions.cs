using System;
using System.Collections.Generic;

namespace NiceAttributes.Editor
{
    public static class SpecialCaseDrawerAttributeExtensions
    {
        private static Dictionary<Type, SpecialCasePropertyDrawerBase> _drawersByAttributeType;

        static SpecialCaseDrawerAttributeExtensions()
        {
            _drawersByAttributeType = new Dictionary<Type, SpecialCasePropertyDrawerBase>();
            _drawersByAttributeType[typeof(ReorderableListAttribute)] = ReorderableListPropertyDrawer.Instance;
        }

        public static SpecialCasePropertyDrawerBase GetDrawer(this SpecialCaseDrawerAttribute attr)
        {
            SpecialCasePropertyDrawerBase drawer;
            if (_drawersByAttributeType.TryGetValue(attr.GetType(), out drawer))
            {
                return drawer;
            }
            else
            {
                return null;
            }
        }
    }
}