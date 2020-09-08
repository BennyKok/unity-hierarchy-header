using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace BK.HierarchyHeader.Editor
{
    public class HeaderSettings : ScriptableObject
    {
        public HeaderType type;

        private static HeaderSettings current;

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
                    settings = HeaderSettings.GetSerializedSettings();
                },
                guiHandler = (searchContext) =>
                {
                    settings.Update();

                    GUILayout.Space(8);

                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Space(12);

                    EditorGUI.BeginChangeCheck();
                    EditorGUILayout.PropertyField(settings.FindProperty("type"), new GUIContent("Default Type"));

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

    // [CustomEditor(typeof(HeaderSettings))]
    // public class HeaderSettingsEditor : UnityEditor.Editor
    // {
    //     public override void OnInspectorGUI()
    //     {
    //         EditorGUILayout.HelpBox("Edit in settings.", MessageType.Info);
    //     }
    // }

}