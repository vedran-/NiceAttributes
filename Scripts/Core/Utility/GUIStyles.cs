using UnityEditor;
using UnityEngine;

namespace NiceAttributes
{
    public static class GUIStyles
    {
        private static GUIStyle _fullRectStyle, _roundedRectStyle, _roundedTopRectStyle,
            _roundedBottomRectStyle;

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
            get
            {
                if (_roundedRectStyle == null)
                {
                    var guids = AssetDatabase.FindAssets("RoundRect t:Sprite");
                    if (guids.Length > 0)
                    {
                        var path = AssetDatabase.GUIDToAssetPath(guids[0]);
                        var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
                        _roundedRectStyle = DrawingUtil.GUIStyleFromSplicedSprite(sprite);
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
                    _roundedTopRectStyle = DrawingUtil.GUIStyleFromSplicedSprite(sprite);
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
                    _roundedBottomRectStyle = DrawingUtil.GUIStyleFromSplicedSprite(sprite);
                }

                return _roundedBottomRectStyle;
            }
        }
    }
}