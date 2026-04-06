#if UNITY_EDITOR
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
            get
            {
                if (_roundedRectStyle == null)
                {
                    if (string.IsNullOrEmpty(_roundedRectGuid))
                    {
                        var guids = AssetDatabase.FindAssets("RoundRect t:Sprite");
                        if (guids.Length > 0)
                        {
                            _roundedRectGuid = guids[0];
                        }
                    }

                    if (!string.IsNullOrEmpty(_roundedRectGuid) && _roundedRectSprite == null)
                    {
                        var path = AssetDatabase.GUIDToAssetPath(_roundedRectGuid);
                        _roundedRectSprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
                    }

                    if (_roundedRectSprite != null)
                    {
                        _roundedRectStyle = GUIUtil.GUIStyleFromSplicedSprite(_roundedRectSprite);
                    }
                }

                return _roundedRectStyle;
            }
        }

        public static GUIStyle RoundedTopRect
        {
            get
            {
                if (_roundedTopRectStyle == null)
                {
                    if (string.IsNullOrEmpty(_roundedTopRectGuid))
                    {
                        var guids = AssetDatabase.FindAssets("RoundTopRect t:Sprite");
                        if (guids.Length > 0)
                        {
                            _roundedTopRectGuid = guids[0];
                        }
                    }

                    if (!string.IsNullOrEmpty(_roundedTopRectGuid) && _roundedTopRectSprite == null)
                    {
                        var path = AssetDatabase.GUIDToAssetPath(_roundedTopRectGuid);
                        _roundedTopRectSprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
                    }

                    if (_roundedTopRectSprite != null)
                    {
                        _roundedTopRectStyle = GUIUtil.GUIStyleFromSplicedSprite(_roundedTopRectSprite);
                    }
                }

                return _roundedTopRectStyle;
            }
        }

        public static GUIStyle RoundedBottomRect
        {
            get
            {
                if (_roundedBottomRectStyle == null)
                {
                    if (string.IsNullOrEmpty(_roundedBottomRectGuid))
                    {
                        var guids = AssetDatabase.FindAssets("RoundBottomRect t:Sprite");
                        if (guids.Length > 0)
                        {
                            _roundedBottomRectGuid = guids[0];
                        }
                    }

                    if (!string.IsNullOrEmpty(_roundedBottomRectGuid) && _roundedBottomRectSprite == null)
                    {
                        var path = AssetDatabase.GUIDToAssetPath(_roundedBottomRectGuid);
                        _roundedBottomRectSprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
                    }

                    if (_roundedBottomRectSprite != null)
                    {
                        _roundedBottomRectStyle = GUIUtil.GUIStyleFromSplicedSprite(_roundedBottomRectSprite);
                    }
                }

                return _roundedBottomRectStyle;
            }
        }
    }
}
#endif