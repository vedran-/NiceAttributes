using UnityEditor;
using UnityEngine;

namespace NiceAttributes
{
    public static class GUIStyles
    {
        private static GUIStyle _roundedRectStyle, _roundedTopRectStyle, _roundedBottomRectStyle;

        public static GUIStyle RoundedRect
        {
            get
            {
                if (_roundedRectStyle == null)
                {
                    var guids = AssetDatabase.FindAssets("RoundRect t:Sprite");
                    if (guids.Length > 0)
                    {
                        var path = AssetDatabase.GUIDToAssetPath(guids[0]);
                        var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
                        _roundedRectStyle = GUIUtil.GUIStyleFromSplicedSprite(sprite);
                    }
                }

                return _roundedRectStyle;
            }
        }

        public static GUIStyle RoundedTopRect
        {
            get
            {
                var guids = AssetDatabase.FindAssets("RoundTopRect t:Sprite");
                if (guids.Length > 0)
                {
                    var path = AssetDatabase.GUIDToAssetPath(guids[0]);
                    var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
                    _roundedTopRectStyle = GUIUtil.GUIStyleFromSplicedSprite(sprite);
                }

                return _roundedTopRectStyle;
            }
        }

        public static GUIStyle RoundedBottomRect
        {
            get
            {
                var guids = AssetDatabase.FindAssets("RoundBottomRect t:Sprite");
                if (guids.Length > 0)
                {
                    var path = AssetDatabase.GUIDToAssetPath(guids[0]);
                    var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
                    _roundedBottomRectStyle = GUIUtil.GUIStyleFromSplicedSprite(sprite);
                }

                return _roundedBottomRectStyle;
            }
        }
    }
}