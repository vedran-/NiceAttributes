using UnityEditor;
using UnityEngine;

namespace NiceAttributes.Editor.Utility
{
    public static class HelpBoxRenderer
    {
        private static GUIStyle _helpBoxTextStyle, _helpBoxStyle;

        private static void CreateHelpBoxStyles(TextAnchor textAlignment = TextAnchor.MiddleLeft)
        {
            if (_helpBoxTextStyle == null) _helpBoxTextStyle = new GUIStyle(GUI.skin.label)
            {
                richText = true,
                wordWrap = true,
                alignment = textAlignment,
                padding = new RectOffset(4, 4, 10, 10)
            };
            if (_helpBoxStyle == null) _helpBoxStyle = new GUIStyle(GUI.skin.window)
            {
                padding = new RectOffset(1, 1, 1, 1),
                margin = new RectOffset(4, 4, 4, 4)
            };
        }

        public static void HelpBox(Rect rect, string message, MessageType messageType,
            UnityEngine.Object context = null, bool logToConsole = false)
        {
            CreateHelpBoxStyles();

            GUI.BeginGroup(rect, _helpBoxStyle);

            Rect iconRect = new Rect(10, rect.height / 2 - 18, 36, 36);
            GUIContent iconContent = null;
            switch (messageType)
            {
                case MessageType.Info: iconContent = EditorGUIUtility.IconContent("console.infoicon"); break;
                case MessageType.Warning: iconContent = EditorGUIUtility.IconContent("console.warnicon"); break;
                case MessageType.Error: iconContent = EditorGUIUtility.IconContent("console.erroricon"); break;
            }
            if (iconContent != null) GUI.Label(iconRect, iconContent);

            Rect textRect = new Rect(46, 0, rect.width - 46, rect.height);
            GUI.Label(textRect, new GUIContent(message), _helpBoxTextStyle);
            GUI.EndGroup();

            if (logToConsole) DebugLogMessage(message, messageType, context);
        }

        public static void HelpBox_Layout(string message, MessageType messageType,
            UnityEngine.Object context = null, bool logToConsole = false)
        {
            CreateHelpBoxStyles();

            EditorGUILayout.BeginVertical(_helpBoxStyle);
            EditorGUILayout.BeginHorizontal();

            GUIContent iconContent = null;
            switch (messageType)
            {
                case MessageType.Info: iconContent = EditorGUIUtility.IconContent("console.infoicon"); break;
                case MessageType.Warning: iconContent = EditorGUIUtility.IconContent("console.warnicon"); break;
                case MessageType.Error: iconContent = EditorGUIUtility.IconContent("console.erroricon"); break;
            }
            if (iconContent != null)
            {
                EditorGUILayout.LabelField(iconContent, GUILayout.Width(36), GUILayout.ExpandHeight(true));
            }

            EditorGUILayout.LabelField(new GUIContent(message), _helpBoxTextStyle, GUILayout.ExpandWidth(true));
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();

            if (logToConsole) DebugLogMessage(message, messageType, context);
        }

        private static void DebugLogMessage(string message, MessageType type, UnityEngine.Object context)
        {
            switch (type)
            {
                case MessageType.None:
                case MessageType.Info:
                    Debug.Log(message, context);
                    break;
                case MessageType.Warning:
                    Debug.LogWarning(message, context);
                    break;
                case MessageType.Error:
                    Debug.LogError(message, context);
                    break;
            }
        }
    }
}
