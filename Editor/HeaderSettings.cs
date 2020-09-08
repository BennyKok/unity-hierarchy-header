using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace BK.HierarchyHeader.Editor
{
    public class HeaderSettings : ScriptableObject
    {
        [Range(10, 60)]
        public int maxLength = 30;

        [Range(0, 10)]
        public int minPrefixLength = 2;

        public HeaderType type;
        public HeaderAlignment alignment;

        private static HeaderSettings current;

        public static HeaderSettings Instance { get => GetOrCreateSettings(); }

        public static HeaderSettings GetOrCreateSettings()
        {
            if (current != null) return current;

            //Locate our header settings
            var ids = AssetDatabase.FindAssets("t:HeaderSettings");
            if (ids.Length > 0)
                current = AssetDatabase.LoadAssetAtPath<HeaderSettings>(AssetDatabase.GUIDToAssetPath(ids[0]));

            if (current == null)
            {
                var path = EditorUtility.SaveFilePanelInProject("Save HeaderSettings as...", "HierarchyHeaderSettings", "asset", "");
                current = ScriptableObject.CreateInstance<HeaderSettings>();
                current.type = HeaderType.Default;
                current.alignment = HeaderAlignment.Center;
                AssetDatabase.CreateAsset(current, path);
                AssetDatabase.SaveAssets();
            }
            return current;
        }

        internal static SerializedObject GetSerializedSettings()
        {
            return new SerializedObject(GetOrCreateSettings());
        }
    }

    static class SettingsRegester
    {
        private static SerializedObject settings;

        [SettingsProvider]
        public static SettingsProvider CreateMyCustomSettingsProvider()
        {
            var provider = new SettingsProvider("Project/BK/Hierarchy Header", SettingsScope.Project)
            {
                label = "Hierarchy Header",
                activateHandler = (_, element) =>
                {
                    Undo.undoRedoPerformed += HeaderUtils.UpdateAllHeader;
                    settings = HeaderSettings.GetSerializedSettings();
                },
                deactivateHandler = () =>
                {
                    Undo.undoRedoPerformed -= HeaderUtils.UpdateAllHeader;
                },
                guiHandler = (searchContext) =>
                {
                    settings.Update();

                    GUILayout.Space(8);

                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Space(12);

                    EditorGUI.BeginChangeCheck();

                    EditorGUILayout.BeginVertical();
                    EditorGUILayout.PropertyField(settings.FindProperty("maxLength"));
                    EditorGUILayout.PropertyField(settings.FindProperty("type"));
                    EditorGUILayout.PropertyField(settings.FindProperty("alignment"));
                    var alignment = settings.FindProperty("alignment");
                    if (alignment.enumValueIndex == 0 || alignment.enumValueIndex == 2)
                    {
                        EditorGUILayout.PropertyField(settings.FindProperty("minPrefixLength"));
                    }
                    EditorGUILayout.EndVertical();

                    GUILayout.Space(12);
                    EditorGUILayout.EndHorizontal();

                    settings.ApplyModifiedProperties();

                    if (EditorGUI.EndChangeCheck())
                    {
                        HeaderUtils.UpdateAllHeader();
                    }
                },

                // Populate the search keywords to enable smart search filtering and label highlighting:
                keywords = new HashSet<string>(new[] { "Header" })
            };

            return provider;
        }
    }
}