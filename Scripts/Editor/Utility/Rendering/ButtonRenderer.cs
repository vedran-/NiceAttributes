using System;
using System.Collections;
using System.Linq;
using NiceAttributes.Editor.PropertyDrawers_SpecialCase;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace NiceAttributes.Editor.Utility
{
    public static class ButtonRenderer
    {
        private static readonly GUIStyle _buttonStyle = new GUIStyle(GUI.skin.button) { richText = true };

        public static void Button(object target, System.Reflection.MethodInfo methodInfo)
        {
            bool visible = ButtonUtility.IsVisible(target, methodInfo);
            if (!visible) return;

            if (methodInfo.GetParameters().All(p => p.IsOptional))
            {
                var buttonAttribute = (ButtonAttribute)methodInfo.GetCustomAttributes(typeof(ButtonAttribute), true)[0];
                var buttonText = string.IsNullOrEmpty(buttonAttribute.Text) ? ObjectNames.NicifyVariableName(methodInfo.Name) : buttonAttribute.Text;
                var buttonEnabled = ButtonUtility.IsEnabled(target, methodInfo);

                var mode = buttonAttribute.SelectedEnableMode;
                buttonEnabled &=
                    mode == ButtonEnableMode.Always ||
                    mode == ButtonEnableMode.Editor && !Application.isPlaying ||
                    mode == ButtonEnableMode.Playmode && Application.isPlaying;

                var methodIsCoroutine = methodInfo.ReturnType == typeof(IEnumerator);
                if (methodIsCoroutine) buttonEnabled &= Application.isPlaying;

                EditorGUI.BeginDisabledGroup(!buttonEnabled);

                if (GUILayout.Button(buttonText, _buttonStyle))
                {
                    var defaultParams = methodInfo.GetParameters().Select(p => p.DefaultValue).ToArray();
                    var methodResult = methodInfo.Invoke(target, defaultParams) as IEnumerator;

                    if (!Application.isPlaying)
                    {
                        var obj = target as UnityEngine.Object;
                        if (obj != null) EditorUtility.SetDirty(obj);

                        PrefabStage stage = PrefabStageUtility.GetCurrentPrefabStage();
                        if (stage != null)
                            EditorSceneManager.MarkSceneDirty(stage.scene);
                        else
                            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
                    }
                    else if (methodResult != null && target is MonoBehaviour behaviour)
                    {
                        behaviour.StartCoroutine(methodResult);
                    }
                }

                EditorGUI.EndDisabledGroup();
            }
            else
            {
                string warning = nameof(ButtonAttribute) + $" works only on methods with no parameters [{methodInfo.Name}]";
                NiceEditorGUI.HelpBox_Layout(warning, MessageType.Warning, context: target as UnityEngine.Object, logToConsole: true);
            }
        }
    }
}
