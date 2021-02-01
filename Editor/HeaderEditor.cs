using System.Text;
using UnityEditor;
using UnityEngine;

namespace BK.HierarchyHeader.Editor
{
    [CustomEditor(typeof(Header))]
    public class HeaderEditor : UnityEditor.Editor
    {
        public static void UpdateAllHeader()
        {
            var targetType = HeaderSettings.headerType;
            var targetAlignment = HeaderSettings.alignment;
            var allHeader = GameObject.FindObjectsOfType<Header>();
            foreach (var header in allHeader)
            {
                header.type = targetType;
                header.alignment = targetAlignment;

                HeaderEditor.UpdateHeader(header, null, true);
            }
        }

        public static string GetSimpleTitle(string prefix, string title)
        {
            if (prefix == null || prefix.Length <= 0) return title;
            return GetSimpleTitle(prefix[0], title);
        }

        public static string GetSimpleTitle(char prefix, string title)
        {
            var maxCharLength = HeaderSettings.maxLength;
            var charLength = maxCharLength - title.Length;

            var leftSize = 0;
            var rightSize = 0;
            switch (HeaderSettings.alignment.value)
            {
                case HeaderAlignment.Start:
                    leftSize = HeaderSettings.minPrefixLength;
                    rightSize = charLength - leftSize;
                    break;
                case HeaderAlignment.End:
                    rightSize = HeaderSettings.minPrefixLength;
                    leftSize = charLength - rightSize;
                    break;
                case HeaderAlignment.Center:
                    leftSize = charLength / 2;
                    rightSize = charLength / 2;
                    break;
            }

            string left = leftSize > 0 ? new string(prefix, leftSize) : "";
            string right = rightSize > 0 ? new string(prefix, rightSize) : "";

            var builder = new StringBuilder();
            builder.Append(left);
            builder.Append(" ");
            builder.Append(title.ToUpper());
            builder.Append(" ");
            builder.Append(right);

            return builder.ToString();
        }

        public static string GetFormattedTitle(string title)
        {
            switch (HeaderSettings.headerType.value)
            {
                case HeaderType.Dotted:
                    return GetSimpleTitle('-', title);
                case HeaderType.Custom:
                    return GetSimpleTitle(HeaderSettings.customPrefix, title);
            }
            return GetSimpleTitle('━', title);
        }

        public static void UpdateHeader(Header header, string title = null, bool markAsDirty = false)
        {
            var targetTitle = title == null ? header.title : title;

            header.name = GetFormattedTitle(targetTitle);

            if (markAsDirty)
                EditorUtility.SetDirty(header);
        }

        private void OnEnable() => Undo.undoRedoPerformed += OnUndoRedo;

        private void OnDisable() => Undo.undoRedoPerformed -= OnUndoRedo;

        public void OnUndoRedo() => UpdateHeader(target as Header, null, true);

        private bool titleChanged;

        private double lastChangedTime;

        public override void OnInspectorGUI()
        {
            var typeProperty = serializedObject.FindProperty("type");

            var header = target as Header;

            serializedObject.Update();

            var titleProperty = serializedObject.FindProperty("title");
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(titleProperty);
            if (EditorGUI.EndChangeCheck())
            {
                UpdateHeader(header, titleProperty.stringValue, false);

                //Refresh the hierarchy to reflect the new name
                EditorApplication.RepaintHierarchyWindow();
            }

            //Sync current header with settings
            if ((HeaderType)typeProperty.enumValueIndex != HeaderSettings.headerType.value)
            {
                typeProperty.enumValueIndex = (int)HeaderSettings.headerType.value;

                UpdateHeader(header, null, false);
            }

            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Options"))
            {
                SettingsService.OpenProjectSettings("Project/BK/Hierarchy Header");
            }
            if (GUILayout.Button("Refresh"))
            {
                UpdateAllHeader();
            }
            if (GUILayout.Button("Create Empty"))
            {
                var o = new GameObject("Empty");
                o.transform.SetSiblingIndex((target as Header).transform.GetSiblingIndex() + 1);
                Undo.RegisterCreatedObjectUndo(o, "Create Empty");
            }
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            serializedObject.ApplyModifiedProperties();
        }
    }
}