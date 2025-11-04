#if UNITY_EDITOR
using UnityEditor;

namespace NiceAttributes
{
    public static class GlobalConfig
    {
        public static bool EnableNiceAttributes = true;

        [MenuItem("Tools/Nice Attributes/Enable Nice Attributes", priority = 1)]
        private static void SetEnableNiceAttributes()
        {
            EnableNiceAttributes = true;
        }

        [MenuItem("Tools/Nice Attributes/Disable Nice Attributes", priority = 2)]
        private static void SetDisableNiceAttributes()
        {
            EnableNiceAttributes = false;
        }
    }
}
#endif
