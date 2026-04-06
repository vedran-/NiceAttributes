#if UNITY_EDITOR
using UnityEditor;

namespace NiceAttributes
{
    public static class GlobalConfig
    {
        private const string PREFS_KEY = "NiceAttributes.Enabled";
        private static bool? _enableNiceAttributes;

        public static bool EnableNiceAttributes
        {
            get
            {
                if (!_enableNiceAttributes.HasValue)
                    _enableNiceAttributes = EditorPrefs.GetBool(PREFS_KEY, true);
                return _enableNiceAttributes.Value;
            }
            set
            {
                _enableNiceAttributes = value;
                EditorPrefs.SetBool(PREFS_KEY, value);
            }
        }

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
