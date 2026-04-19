using UnityEditor;
using UnityEngine;

namespace NiceAttributes
{
    public static class GUIStyles
    {
        private static GUIStyle _fullRectStyle, _roundedRectStyle, _roundedTopRectStyle,
            _roundedBottomRectStyle;

        private static string _roundedRectGuid, _roundedTopRectGuid, _roundedBottomRectGuid;
        private static Sprite _roundedRectSprite, _roundedTopRectSprite, _roundedBottomRectSprite;

        public static GUIStyle FullRect
        {
            get
            {
                if (_fullRectStyle == null)
                {
                    _fullRectStyle = new GUIStyle();
                    _fullRectStyle.normal.background = EditorGUIUtility.whiteTexture;
                }

                return _fullRectStyle;
            }
        }

        public static GUIStyle RoundedRect
        {
            get => LoadStyleFromSprite(ref _roundedRectGuid, ref _roundedRectSprite, ref _roundedRectStyle, "RoundRect t:Sprite");
        }

        public static GUIStyle RoundedTopRect
        {
            get => LoadStyleFromSprite(ref _roundedTopRectGuid, ref _roundedTopRectSprite, ref _roundedTopRectStyle, "RoundTopRect t:Sprite");
        }

        public static GUIStyle RoundedBottomRect
        {
            get => LoadStyleFromSprite(ref _roundedBottomRectGuid, ref _roundedBottomRectSprite, ref _roundedBottomRectStyle, "RoundBottomRect t:Sprite");
        }

        private static GUIStyle LoadStyleFromSprite(ref string guidCache, ref Sprite spriteCache, ref GUIStyle styleCache, string searchPattern)
        {
            if (styleCache == null)
            {
                if (string.IsNullOrEmpty(guidCache))
                {
                    var guids = AssetDatabase.FindAssets(searchPattern);
                    if (guids.Length > 0)
                    {
                        guidCache = guids[0];
                    }
                }

                if (!string.IsNullOrEmpty(guidCache) && spriteCache == null)
                {
                    var path = AssetDatabase.GUIDToAssetPath(guidCache);
                    spriteCache = AssetDatabase.LoadAssetAtPath<Sprite>(path);
                }

                if (spriteCache != null)
                {
                    styleCache = GUIUtil.GUIStyleFromSplicedSprite(spriteCache);
                }
            }

            return styleCache;
        }
    }
}